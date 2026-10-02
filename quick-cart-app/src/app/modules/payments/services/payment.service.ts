import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import {
  CreatePaymentIntentRequest,
  PaymentDto,
  PaymentIntentResponse,
  VerifyPaymentRequest,
} from '../models/payment.model';

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  private readonly api = inject(ApiService);

  readonly activeIntent = signal<PaymentIntentResponse | null>(null);
  readonly latestPayment = signal<PaymentDto | null>(null);

  createPaymentIntent(request: CreatePaymentIntentRequest): Observable<PaymentIntentResponse> {
    return this.api
      .post<PaymentIntentResponse>('payments/intents', request)
      .pipe(tap((intent) => this.activeIntent.set(intent)));
  }

  verifyPayment(request: VerifyPaymentRequest): Observable<PaymentDto> {
    return this.api
      .post<PaymentDto>('payments/verify', request)
      .pipe(tap((payment) => this.latestPayment.set(payment)));
  }

  getPaymentByOrderId(orderId: string): Observable<PaymentDto> {
    return this.api
      .get<PaymentDto>(`payments/order/${orderId}`)
      .pipe(tap((payment) => this.latestPayment.set(payment)));
  }

  retryPayment(paymentId: string): Observable<PaymentIntentResponse> {
    return this.api
      .post<PaymentIntentResponse>(`payments/${paymentId}/retry`, {})
      .pipe(tap((intent) => this.activeIntent.set(intent)));
  }
}
