namespace Payment.Tests.Services;

using BuildingBlocks.Application.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Ordering.Application.DTOs;
using Ordering.Application.Services;
using Payment.Application.DTOs;
using Payment.Application.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Enums;
using Payment.Infrastructure.Options;
using Payment.Infrastructure.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _repositoryMock = new();
    private readonly Mock<IOrderService> _orderServiceMock = new();
    private readonly IPaymentGatewayService _gatewayService;
    private readonly PaymentService _sut;

    public PaymentServiceTests()
    {
        var options = Options.Create(new PaymentGatewayOptions
        {
            ProviderName = "QuickCartPay",
            KeyId = "qcp_test_key",
            WebhookSecret = "Test_HMAC_Webhook_Secret_Key_2026_Enterprise!",
            ExposeDevSignature = true
        });

        _gatewayService = new PaymentGatewayService(options);
        _sut = new PaymentService(
            _repositoryMock.Object,
            _gatewayService,
            _orderServiceMock.Object,
            NullLogger<PaymentService>.Instance);
    }

    private static OrderDto CreateSampleOrder(Guid orderId, Guid userId, decimal totalAmount = 249.50m, string status = "Placed")
    {
        return new OrderDto(
            orderId,
            "QC-20260927-1001",
            userId,
            Guid.NewGuid(),
            status,
            totalAmount,
            0m,
            0m,
            totalAmount,
            DateTime.UtcNow,
            10,
            Array.Empty<OrderItemDto>(),
            Array.Empty<OrderStatusHistoryDto>());
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_UsesAuthoritativeServerOrderAmount_AndSavesPayment()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var order = CreateSampleOrder(orderId, userId, totalAmount: 385.75m);

        _orderServiceMock
            .Setup(o => o.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        Payment? persistedPayment = null;
        _repositoryMock
            .Setup(r => r.AddPaymentAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((p, _) => persistedPayment = p)
            .Returns(Task.CompletedTask);

        var request = new CreatePaymentIntentRequest(orderId, "UPI", "idem-key-001");

        // Act
        var response = await _sut.CreatePaymentIntentAsync(userId, request);

        // Assert
        Assert.Equal(orderId, response.OrderId);
        Assert.Equal(385.75m, response.Amount);
        Assert.Equal("INR", response.Currency);
        Assert.Equal("UPI", response.PaymentMethod);
        Assert.Equal("Initiated", response.Status);
        Assert.False(response.IsIdempotentReplay);
        Assert.NotNull(response.DevVerificationSignature);
        Assert.NotNull(persistedPayment);
        Assert.Equal(385.75m, persistedPayment!.Amount);
        Assert.Single(persistedPayment.AuditLogs);
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_DuplicateIdempotencyKey_ReturnsExistingWithoutDuplicateInsert()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var existingPayment = Payment.Create(
            orderId,
            userId,
            199.00m,
            "INR",
            PaymentMethodType.UPI,
            "QuickCartPay",
            "order_existing_123",
            "idem-dup-001");

        _repositoryMock
            .Setup(r => r.GetByIdempotencyKeyAsync("idem-dup-001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPayment);

        var request = new CreatePaymentIntentRequest(orderId, "UPI", "idem-dup-001");

        // Act
        var response = await _sut.CreatePaymentIntentAsync(userId, request);

        // Assert
        Assert.True(response.IsIdempotentReplay);
        Assert.Equal(existingPayment.Id, response.PaymentId);
        Assert.Equal("order_existing_123", response.ProviderOrderId);
        _repositoryMock.Verify(r => r.AddPaymentAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_WhenOrderAlreadyCaptured_ThrowsConflictException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var capturedPayment = Payment.Create(
            orderId,
            userId,
            150.00m,
            "INR",
            PaymentMethodType.Card,
            "QuickCartPay",
            "order_paid_123",
            "idem-paid-1");
        capturedPayment.MarkCaptured("pay_999");

        _repositoryMock
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(capturedPayment);

        var request = new CreatePaymentIntentRequest(orderId, "CARD", "idem-new-attempt");

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _sut.CreatePaymentIntentAsync(userId, request));
    }

    [Fact]
    public async Task VerifyPaymentAsync_ValidHmacSignature_CapturesPaymentAndConfirmsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var providerOrderId = "order_valid_hmac_01";
        var providerPaymentId = "pay_valid_hmac_01";
        var validSignature = _gatewayService.ComputeCheckoutSignature(providerOrderId, providerPaymentId);

        var payment = Payment.Create(
            orderId,
            userId,
            420.00m,
            "INR",
            PaymentMethodType.UPI,
            "QuickCartPay",
            providerOrderId,
            "idem-verify-1");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var request = new VerifyPaymentRequest(payment.Id, providerOrderId, providerPaymentId, validSignature);

        // Act
        var dto = await _sut.VerifyPaymentAsync(userId, request);

        // Assert
        Assert.Equal("Captured", dto.Status);
        Assert.Equal(providerPaymentId, dto.ProviderPaymentId);
        _orderServiceMock.Verify(
            o => o.UpdateOrderStatusAsync(orderId, "Confirmed", It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task VerifyPaymentAsync_InvalidSignature_MarksPaymentFailedAndThrowsUnauthorized()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var providerOrderId = "order_tampered_01";
        var payment = Payment.Create(
            orderId,
            userId,
            420.00m,
            "INR",
            PaymentMethodType.Card,
            "QuickCartPay",
            providerOrderId,
            "idem-tampered-1");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var request = new VerifyPaymentRequest(payment.Id, providerOrderId, "pay_fake_01", "deadbeef_invalid_signature");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _sut.VerifyPaymentAsync(userId, request));
        Assert.Equal(PaymentStatus.Failed, payment.Status);
        _orderServiceMock.Verify(
            o => o.UpdateOrderStatusAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_ValidSignature_CapturesPaymentAndRecordsWebhookEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var providerOrderId = "order_wh_100";
        var payment = Payment.Create(
            orderId,
            userId,
            299.00m,
            "INR",
            PaymentMethodType.UPI,
            "QuickCartPay",
            providerOrderId,
            "idem-wh-100");

        _repositoryMock
            .Setup(r => r.WebhookEventExistsAsync("evt_wh_001", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(r => r.GetByProviderOrderIdAsync(providerOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var rawJson = "{\"eventId\":\"evt_wh_001\",\"eventType\":\"payment.captured\",\"providerOrderId\":\"order_wh_100\",\"providerPaymentId\":\"pay_wh_100\",\"amount\":299.00,\"currency\":\"INR\"}";
        var signature = _gatewayService.ComputeWebhookSignature(rawJson);
        var payload = new PaymentWebhookPayload(
            "evt_wh_001",
            "payment.captured",
            providerOrderId,
            "pay_wh_100",
            299.00m,
            "INR",
            null,
            DateTime.UtcNow);

        // Act
        var result = await _sut.ProcessWebhookAsync(rawJson, signature, payload);

        // Assert
        Assert.True(result.Processed);
        Assert.False(result.IsIdempotentReplay);
        Assert.Equal("Captured", result.Status);
        Assert.Equal(PaymentStatus.Captured, payment.Status);
        _repositoryMock.Verify(r => r.AddWebhookEventAsync(It.IsAny<PaymentWebhookEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _orderServiceMock.Verify(o => o.UpdateOrderStatusAsync(orderId, "Confirmed", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessWebhookAsync_DuplicateEventId_ReturnsIdempotentReplayWithoutReprocessing()
    {
        // Arrange
        var rawJson = "{\"eventId\":\"evt_dup_999\"}";
        var signature = _gatewayService.ComputeWebhookSignature(rawJson);
        var payload = new PaymentWebhookPayload(
            "evt_dup_999",
            "payment.captured",
            "order_wh_dup",
            "pay_wh_dup",
            100.00m,
            "INR",
            null,
            DateTime.UtcNow);

        _repositoryMock
            .Setup(r => r.WebhookEventExistsAsync("evt_dup_999", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ProcessWebhookAsync(rawJson, signature, payload);

        // Assert
        Assert.True(result.Processed);
        Assert.True(result.IsIdempotentReplay);
        Assert.Equal("DuplicateIgnored", result.Status);
        _repositoryMock.Verify(r => r.GetByProviderOrderIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessWebhookAsync_AmountMismatch_MarksPaymentFailedAndThrowsConflict()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var providerOrderId = "order_wh_mismatch";
        var payment = Payment.Create(
            orderId,
            userId,
            500.00m,
            "INR",
            PaymentMethodType.Card,
            "QuickCartPay",
            providerOrderId,
            "idem-wh-mismatch");

        _repositoryMock
            .Setup(r => r.WebhookEventExistsAsync("evt_mismatch_1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(r => r.GetByProviderOrderIdAsync(providerOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        var rawJson = "{\"eventId\":\"evt_mismatch_1\",\"amount\":10.00}";
        var signature = _gatewayService.ComputeWebhookSignature(rawJson);
        var payload = new PaymentWebhookPayload(
            "evt_mismatch_1",
            "payment.captured",
            providerOrderId,
            "pay_mismatch_1",
            10.00m, // Attacker/tampered amount != 500.00m
            "INR",
            null,
            DateTime.UtcNow);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _sut.ProcessWebhookAsync(rawJson, signature, payload));
        Assert.Equal(PaymentStatus.Failed, payment.Status);
    }

    [Fact]
    public async Task RetryPaymentAsync_FailedPayment_IncrementsRetryCountAndReinitiates()
    {
        // Arrange
        var payment = Payment.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            180.00m,
            "INR",
            PaymentMethodType.UPI,
            "QuickCartPay",
            "order_initial_fail",
            "idem-retry-1");
        payment.MarkFailed("Timeout");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act
        var response = await _sut.RetryPaymentAsync(payment.UserId, payment.Id);

        // Assert
        Assert.Equal("Initiated", response.Status);
        Assert.Equal(1, payment.RetryCount);
        Assert.NotEqual("order_initial_fail", response.ProviderOrderId);
    }

    [Fact]
    public async Task RefundPaymentAsync_PartialAndFullRefund_UpdatesStatusAndRefundedAmount()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var payment = Payment.Create(
            orderId,
            Guid.NewGuid(),
            200.00m,
            "INR",
            PaymentMethodType.Card,
            "QuickCartPay",
            "order_refund_01",
            "idem-refund-01");
        payment.MarkCaptured("pay_captured_01");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act 1: Partial refund of 80.00
        var partialDto = await _sut.RefundPaymentAsync(payment.Id, new RefundPaymentRequest(80.00m, "Damaged item"));
        Assert.Equal("PartiallyRefunded", partialDto.Status);
        Assert.Equal(80.00m, partialDto.RefundedAmount);

        // Act 2: Remaining refund of 120.00
        var fullDto = await _sut.RefundPaymentAsync(payment.Id, new RefundPaymentRequest(120.00m, "Order cancelled"));
        Assert.Equal("Refunded", fullDto.Status);
        Assert.Equal(200.00m, fullDto.RefundedAmount);
        _orderServiceMock.Verify(o => o.UpdateOrderStatusAsync(orderId, "Cancelled", It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void PaymentGatewayOptions_DefaultExposeDevSignature_IsFalse()
    {
        var options = new PaymentGatewayOptions();
        Assert.False(options.ExposeDevSignature);
    }

    [Fact]
    public async Task CreatePaymentIntentAsync_WhenUserDoesNotOwnOrder_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var orderOwnerId = Guid.NewGuid();
        var callerUserId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var order = CreateSampleOrder(orderId, orderOwnerId);

        _orderServiceMock
            .Setup(o => o.GetOrderByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.CreatePaymentIntentAsync(callerUserId, new CreatePaymentIntentRequest(orderId, "UPI")));
        Assert.Equal("Order does not belong to the current user.", ex.Message);
    }

    [Fact]
    public async Task VerifyPaymentAsync_WhenUserDoesNotOwnPayment_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var paymentOwnerId = Guid.NewGuid();
        var callerUserId = Guid.NewGuid();
        var payment = Payment.Create(
            Guid.NewGuid(),
            paymentOwnerId,
            100m,
            "INR",
            PaymentMethodType.UPI,
            "QuickCartPay",
            "order_gateway_1",
            "idem-key-1");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.VerifyPaymentAsync(callerUserId, new VerifyPaymentRequest(payment.Id, "order_gateway_1", "pay_1", "sig_1")));
        Assert.Equal("Payment does not belong to the current user.", ex.Message);
    }

    [Fact]
    public async Task RetryPaymentAsync_WhenUserDoesNotOwnPayment_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var paymentOwnerId = Guid.NewGuid();
        var callerUserId = Guid.NewGuid();
        var payment = Payment.Create(
            Guid.NewGuid(),
            paymentOwnerId,
            100m,
            "INR",
            PaymentMethodType.UPI,
            "QuickCartPay",
            "order_gateway_retry",
            "idem-key-retry");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(payment);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _sut.RetryPaymentAsync(callerUserId, payment.Id));
        Assert.Equal("Payment does not belong to the current user.", ex.Message);
    }
}
