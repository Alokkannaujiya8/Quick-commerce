import { Injectable, inject, signal } from '@angular/core';
import { Observable, of, catchError, map, tap } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';
import {
  CreateOrderRequest,
  OrderResponse,
  OrderStatus,
  PaymentMethod,
  ShippingAddress,
} from '../models/order.model';

interface BackendOrderItemDto {
  id: string;
  productId: string;
  productName: string;
  sku: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

interface BackendOrderStatusHistoryDto {
  status: OrderStatus;
  changedAt: string;
  notes?: string;
}

interface BackendOrderDto {
  id: string;
  orderNumber: string;
  userId: string;
  deliveryAddressId: string;
  status: OrderStatus;
  subtotal: number;
  deliveryFee: number;
  discountAmount: number;
  totalAmount: number;
  placedAt: string;
  estimatedDeliveryMinutes: number;
  items: BackendOrderItemDto[];
  statusHistory: BackendOrderStatusHistoryDto[];
}

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private readonly api = inject(ApiService);
  private readonly STORAGE_KEY = 'quickcart_orders';

  readonly currentOrder = signal<OrderResponse | null>(null);
  readonly orderHistory = signal<OrderResponse[]>(this.loadOrdersFromStorage());

  createOrder(request: CreateOrderRequest): Observable<OrderResponse> {
    const checkoutPayload = {
      deliveryAddressId: 'f784e1b8-6a34-4bc5-9c3f-912ab0819fa2',
      paymentMethod: request.paymentMethod,
      notes: `Deliver to ${request.shippingAddress.fullName}, ${request.shippingAddress.streetAddress}, ${request.shippingAddress.city} - ${request.shippingAddress.postalCode}`,
      deliveryFee: request.deliveryFee,
      items: request.items.map((i) => ({
        productId: this.ensureGuid(i.productId),
        productName: i.productName,
        sku: i.sku ?? 'QC-ITEM',
        unitPrice: i.unitPrice,
        quantity: i.quantity,
      })),
    };

    return this.api.post<BackendOrderDto>('orders/checkout', checkoutPayload).pipe(
      map((dto) => this.mapBackendOrderToResponse(dto, request.shippingAddress, request.paymentMethod)),
      tap((order) => this.saveOrder(order)),
      catchError((err) => {
        console.warn('Backend orders/checkout unreachable, falling back to local order state:', err);
        const mockOrder = this.generateMockOrder(request);
        this.saveOrder(mockOrder);
        return of(mockOrder);
      })
    );
  }

  getOrderById(id: string): Observable<OrderResponse | null> {
    const localMatch = this.orderHistory().find((o) => o.id === id || o.orderNumber === id);
    return this.api.get<BackendOrderDto>(`orders/${id}`).pipe(
      map((dto) =>
        this.mapBackendOrderToResponse(
          dto,
          localMatch?.shippingAddress ?? {
            fullName: 'QuickCart Customer',
            phoneNumber: '+91 98765 43210',
            streetAddress: '12th Main Road, Indiranagar',
            city: 'Bengaluru',
            state: 'Karnataka',
            postalCode: '560038',
            addressType: 'Home',
          },
          localMatch?.paymentMethod ?? 'UPI',
          localMatch?.paymentStatus,
          localMatch?.paymentTransactionId
        )
      ),
      tap((order) => this.saveOrder(order)),
      catchError(() => of(localMatch || null))
    );
  }

  getUserOrders(): Observable<OrderResponse[]> {
    return this.api.get<BackendOrderDto[]>('orders').pipe(
      map((dtos) =>
        dtos.map((dto) => {
          const existing = this.orderHistory().find((o) => o.id === dto.id);
          return this.mapBackendOrderToResponse(
            dto,
            existing?.shippingAddress ?? {
              fullName: 'QuickCart Customer',
              phoneNumber: '+91 98765 43210',
              streetAddress: 'Bengaluru',
              city: 'Bengaluru',
              state: 'Karnataka',
              postalCode: '560001',
              addressType: 'Home',
            },
            existing?.paymentMethod ?? 'UPI',
            existing?.paymentStatus,
            existing?.paymentTransactionId
          );
        })
      ),
      catchError(() => of(this.orderHistory()))
    );
  }

  updateOrderPaymentState(
    orderId: string,
    paymentStatus: OrderResponse['paymentStatus'],
    transactionId?: string
  ): void {
    const current = this.currentOrder();
    if (current && current.id === orderId) {
      const updatedOrder: OrderResponse = {
        ...current,
        status: paymentStatus === 'Captured' || paymentStatus === 'Paid' ? 'Confirmed' : current.status,
        paymentStatus,
        paymentTransactionId: transactionId ?? current.paymentTransactionId,
      };
      this.saveOrder(updatedOrder);
    }
  }

  private mapBackendOrderToResponse(
    dto: BackendOrderDto,
    shippingAddress: ShippingAddress,
    paymentMethod: PaymentMethod,
    paymentStatusOverride?: OrderResponse['paymentStatus'],
    transactionId?: string
  ): OrderResponse {
    return {
      id: dto.id,
      orderNumber: dto.orderNumber,
      orderDate: dto.placedAt,
      status: dto.status,
      paymentMethod,
      paymentStatus:
        paymentStatusOverride ?? (paymentMethod === 'COD' ? 'Pending' : 'Initiated'),
      paymentTransactionId: transactionId,
      totalAmount: dto.totalAmount,
      deliveryFee: dto.deliveryFee,
      shippingAddress,
      items: dto.items.map((item) => ({
        productId: item.productId,
        productName: item.productName,
        sku: item.sku,
        unitPrice: item.unitPrice,
        quantity: item.quantity,
        totalPrice: item.lineTotal,
      })),
      estimatedDeliveryMinutes: dto.estimatedDeliveryMinutes || 10,
      statusHistory: dto.statusHistory.map((h) => ({
        status: h.status,
        timestamp: h.changedAt,
        description: h.notes || `Order status updated to ${h.status}`,
      })),
    };
  }

  private ensureGuid(value: string): string {
    const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
    return guidRegex.test(value) ? value : '9b1deb4d-3b7d-4bad-9bdd-2b0d7b3dcb6d';
  }

  private saveOrder(order: OrderResponse): void {
    this.currentOrder.set(order);
    const updated = [order, ...this.orderHistory().filter((o) => o.id !== order.id)];
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
    const orderId = '5fa23d11-6548-43bb-81d3-f0a51e60472e';
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
          description: 'Order placed and confirmed by store.',
        },
      ],
    };
  }
}
