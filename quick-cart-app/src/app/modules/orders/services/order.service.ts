import { Injectable, inject, signal } from '@angular/core';
import { Observable, of, catchError, map, tap } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { CreateOrderRequest, OrderResponse, OrderStatus } from '../models/order.model';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);

  private readonly STORAGE_KEY = 'quickcart_orders';

  readonly currentOrder = signal<OrderResponse | null>(null);
  readonly orderHistory = signal<OrderResponse[]>(this.loadOrdersFromStorage());

  /**
   * Submit new order to backend with resilient local fallback
   */
  createOrder(request: CreateOrderRequest): Observable<OrderResponse> {
    return this.api.post<OrderResponse>('orders', request).pipe(
      tap(order => {
        this.saveOrder(order);
      }),
      catchError(err => {
        console.warn('Backend orders API unreachable, generating simulated order for demo:', err);
        const mockOrder = this.generateMockOrder(request);
        this.saveOrder(mockOrder);
        return of(mockOrder);
      })
    );
  }

  /**
   * Get order by ID
   */
  getOrderById(id: string): Observable<OrderResponse | null> {
    return this.api.get<OrderResponse>(`orders/${id}`).pipe(
      catchError(() => {
        const local = this.orderHistory().find(o => o.id === id || o.orderNumber === id);
        return of(local || null);
      })
    );
  }

  /**
   * Get orders for the logged-in user
   */
  getUserOrders(): Observable<OrderResponse[]> {
    return this.api.get<OrderResponse[]>('orders').pipe(
      catchError(() => of(this.orderHistory()))
    );
  }

  private saveOrder(order: OrderResponse): void {
    this.currentOrder.set(order);
    const updated = [order, ...this.orderHistory().filter(o => o.id !== order.id)];
    this.orderHistory.set(updated);
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(updated));
    }
  }

  private loadOrdersFromStorage(): OrderResponse[] {
    if (typeof localStorage === 'undefined') return [];
    try {
      const data = localStorage.getItem(this.STORAGE_KEY);
      return data ? JSON.parse(data) : [];
    } catch {
      return [];
    }
  }

  private generateMockOrder(req: CreateOrderRequest): OrderResponse {
    const orderNum = 'QC-' + Math.floor(100000 + Math.random() * 900000);
    const orderId = 'ord-' + Date.now();
    const now = new Date().toISOString();

    return {
      id: orderId,
      orderNumber: orderNum,
      orderDate: now,
      status: 'Confirmed',
      paymentMethod: req.paymentMethod,
      paymentStatus: req.paymentMethod === 'COD' ? 'Pending' : 'Paid',
      totalAmount: req.totalAmount,
      deliveryFee: req.deliveryFee,
      shippingAddress: req.shippingAddress,
      items: req.items,
      estimatedDeliveryMinutes: 10,
      statusHistory: [
        {
          status: 'Confirmed',
          timestamp: now,
          description: 'Order placed and confirmed by store.'
        },
        {
          status: 'Processing',
          timestamp: new Date(Date.now() + 60000).toISOString(),
          description: 'Items are being packed at nearest dark store.'
        }
      ]
    };
  }
}
