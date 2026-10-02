export type OrderStatus =
  | 'Placed'
  | 'Pending'
  | 'Confirmed'
  | 'Packed'
  | 'Processing'
  | 'OutForDelivery'
  | 'Delivered'
  | 'Cancelled';

export type PaymentMethod = 'COD' | 'UPI' | 'CARD' | 'WALLET';

export interface OrderItemDto {
  productId: string;
  productName: string;
  sku?: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
  imageUrl?: string;
  unitOfMeasure?: string;
}

export interface ShippingAddress {
  fullName: string;
  phoneNumber: string;
  streetAddress: string;
  city: string;
  state: string;
  postalCode: string;
  addressType: 'Home' | 'Work' | 'Other';
}

export interface CreateOrderRequest {
  shippingAddress: ShippingAddress;
  paymentMethod: PaymentMethod;
  items: OrderItemDto[];
  subtotal: number;
  deliveryFee: number;
  totalAmount: number;
  specialInstructions?: string;
}

export interface OrderStatusHistoryItem {
  status: OrderStatus;
  timestamp: string;
  description: string;
}

export interface OrderResponse {
  id: string;
  orderNumber: string;
  orderDate: string;
  status: OrderStatus;
  paymentMethod: PaymentMethod;
  paymentStatus: 'Pending' | 'Initiated' | 'Captured' | 'Paid' | 'Failed';
  paymentTransactionId?: string;
  totalAmount: number;
  deliveryFee: number;
  shippingAddress: ShippingAddress;
  items: OrderItemDto[];
  estimatedDeliveryMinutes: number;
  statusHistory: OrderStatusHistoryItem[];
}
