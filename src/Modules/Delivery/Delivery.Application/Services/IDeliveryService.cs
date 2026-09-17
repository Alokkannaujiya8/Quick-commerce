namespace Delivery.Application.Services;

using Delivery.Application.DTOs;

public interface IDeliveryService
{
    Task<DeliveryTrackingDto> AssignRiderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<DeliveryTrackingDto?> GetTrackingByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task UpdateRiderLocationAsync(RiderLocationUpdate update, CancellationToken cancellationToken = default);
}

