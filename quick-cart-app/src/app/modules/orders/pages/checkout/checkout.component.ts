import { ChangeDetectionStrategy, Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { switchMap, of, catchError } from 'rxjs';
import { CartService } from '../../../cart/services/cart.service';
import { OrderService } from '../../services/order.service';
import { PaymentService } from '../../../payments/services/payment.service';
import { AuthService } from '../../../../core/auth/auth-service';
import { CreateOrderRequest, PaymentMethod, ShippingAddress } from '../../models/order.model';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CheckoutComponent implements OnInit {
  readonly cart = inject(CartService);
  private readonly orderService = inject(OrderService);
  private readonly paymentService = inject(PaymentService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly isSubmitting = signal(false);
  readonly paymentStepMessage = signal('Creating Order...');
  readonly errorMessage = signal<string | null>(null);
  selectedPayment: PaymentMethod = 'UPI';

  address: ShippingAddress = {
    fullName: '',
    phoneNumber: '',
    streetAddress: '',
    city: 'Bengaluru',
    state: 'Karnataka',
    postalCode: '560001',
    addressType: 'Home',
  };

  ngOnInit(): void {
    const user = this.authService.currentUser();
    if (user) {
      this.address.fullName = user.fullName || '';
      this.address.phoneNumber = user.phoneNumber || '';
    }
  }

  placeOrder(): void {
    if (!this.address.fullName || !this.address.phoneNumber || !this.address.streetAddress) {
      this.errorMessage.set(
        'Please fill in your recipient name, phone number, and delivery street address.'
      );
      return;
    }

    this.errorMessage.set(null);
    this.isSubmitting.set(true);
    this.paymentStepMessage.set('Creating Order...');

    const itemsDto = this.cart.items().map((item) => ({
      productId: item.productId,
      productName: item.name,
      unitPrice: item.price,
      quantity: item.quantity,
      totalPrice: item.price * item.quantity,
      imageUrl: item.imageUrl,
      unitOfMeasure: item.unitOfMeasure,
    }));

    const orderRequest: CreateOrderRequest = {
      shippingAddress: this.address,
      paymentMethod: this.selectedPayment,
      items: itemsDto,
      subtotal: this.cart.subtotal(),
      deliveryFee: this.cart.deliveryFee(),
      totalAmount: this.cart.totalAmount(),
    };

    this.orderService
      .createOrder(orderRequest)
      .pipe(
        switchMap((order) => {
          this.paymentStepMessage.set('Initiating Secure Payment...');
          const idempotencyKey = `idem-${order.id}-${this.selectedPayment}`;

          return this.paymentService
            .createPaymentIntent({
              orderId: order.id,
              paymentMethod: this.selectedPayment,
              idempotencyKey,
            })
            .pipe(
              switchMap((intent) => {
                if (this.selectedPayment === 'COD') {
                  this.orderService.updateOrderPaymentState(order.id, 'Pending', intent.providerOrderId);
                  return of(order);
                }

                this.paymentStepMessage.set('Verifying Payment Signature...');
                const simulatedPaymentId = `pay_${order.id.replace(/-/g, '')}`.slice(0, 20);

                return this.paymentService
                  .verifyPayment({
                    paymentId: intent.paymentId,
                    providerOrderId: intent.providerOrderId,
                    providerPaymentId: simulatedPaymentId,
                    providerSignature: intent.devVerificationSignature ?? '',
                  })
                  .pipe(
                    switchMap((verifiedPayment) => {
                      this.orderService.updateOrderPaymentState(
                        order.id,
                        'Captured',
                        verifiedPayment.providerPaymentId ?? verifiedPayment.providerOrderId
                      );
                      return of(order);
                    })
                  );
              }),
              catchError((err) => {
                console.warn('Payment API step fallback:', err);
                return of(order);
              })
            );
        })
      )
      .subscribe({
        next: (order) => {
          this.cart.clearCart();
          this.isSubmitting.set(false);
          this.router.navigate(['/order-success', order.id]);
        },
        error: () => {
          this.isSubmitting.set(false);
          this.errorMessage.set('Order processing encountered an issue. Please try again.');
        },
      });
  }
}
