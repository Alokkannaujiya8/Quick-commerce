import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PaymentService } from './payment.service';

describe('PaymentService', () => {
  let service: PaymentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [PaymentService, provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(PaymentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('creates a payment intent and updates activeIntent signal', () => {
    const mockIntent = {
      paymentId: 'pay-1',
      orderId: 'ord-1',
      amount: 249,
      currency: 'INR',
      paymentMethod: 'UPI',
      status: 'Initiated' as const,
      providerName: 'QuickCartPay',
      providerOrderId: 'order_abc',
      providerKeyId: 'qcp_test',
      devVerificationSignature: 'sig_123',
      isIdempotentReplay: false,
      createdAt: new Date().toISOString(),
    };

    service
      .createPaymentIntent({ orderId: 'ord-1', paymentMethod: 'UPI', idempotencyKey: 'idem-1' })
      .subscribe((res) => {
        expect(res).toEqual(mockIntent);
        expect(service.activeIntent()).toEqual(mockIntent);
      });

    const req = httpMock.expectOne((r) => r.url.endsWith('/api/payments/intents'));
    expect(req.request.method).toBe('POST');
    req.flush(mockIntent);
  });

  it('verifies payment signature and updates latestPayment signal', () => {
    const mockPayment = {
      id: 'pay-1',
      orderId: 'ord-1',
      userId: 'usr-1',
      amount: 249,
      refundedAmount: 0,
      currency: 'INR',
      paymentMethod: 'UPI',
      status: 'Captured' as const,
      providerName: 'QuickCartPay',
      providerOrderId: 'order_abc',
      providerPaymentId: 'pay_xyz',
      retryCount: 0,
      createdAt: new Date().toISOString(),
      auditLogs: [],
    };

    service
      .verifyPayment({
        paymentId: 'pay-1',
        providerOrderId: 'order_abc',
        providerPaymentId: 'pay_xyz',
        providerSignature: 'sig_123',
      })
      .subscribe((res) => {
        expect(res.status).toBe('Captured');
        expect(service.latestPayment()?.providerPaymentId).toBe('pay_xyz');
      });

    const req = httpMock.expectOne((r) => r.url.endsWith('/api/payments/verify'));
    expect(req.request.method).toBe('POST');
    req.flush(mockPayment);
  });
});
