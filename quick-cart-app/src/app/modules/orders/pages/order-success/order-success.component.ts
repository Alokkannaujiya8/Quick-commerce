import { ChangeDetectionStrategy, Component, computed, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, of } from 'rxjs';
import { OrderService } from '../../services/order.service';
import { PaymentService } from '../../../payments/services/payment.service';
import { OrderResponse } from '../../models/order.model';
import { PaymentDto } from '../../../payments/models/payment.model';

@Component({
  selector: 'app-order-success',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './order-success.component.html',
  styleUrl: './order-success.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrderSuccessComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly orderService = inject(OrderService);
  private readonly paymentService = inject(PaymentService);

  readonly order = signal<OrderResponse | null>(null);
  readonly paymentDetails = signal<PaymentDto | null>(null);

  readonly paymentBadgeLabel = computed(() => {
    const p = this.paymentDetails();
    if (p) {
      return p.status === 'Captured' ? 'Paid • Verified' : p.status;
    }
    return this.order()?.paymentStatus ?? 'Pending';
  });

  readonly paymentReference = computed(() => {
    const p = this.paymentDetails();
    if (p) {
      return p.providerPaymentId || p.providerOrderId;
    }
    return this.order()?.paymentTransactionId ?? null;
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.orderService.getOrderById(id).subscribe((res) => {
        if (res) {
          this.order.set(res);
        } else if (this.orderService.currentOrder()) {
          this.order.set(this.orderService.currentOrder());
        }
      });

      this.paymentService
        .getPaymentByOrderId(id)
        .pipe(catchError(() => of(null)))
        .subscribe((payment) => {
          if (payment) {
            this.paymentDetails.set(payment);
          }
        });
    } else {
      this.order.set(this.orderService.currentOrder());
    }
  }
}
