export interface CartItem {
  id: string;
  productId: string;
  name: string;
  sku: string;
  unitOfMeasure: string;
  price: number;
  imageUrl?: string;
  quantity: number;
  lineTotal: number;
}

export interface CartSummary {
  items: CartItem[];
  subtotal: number;
  deliveryFee: number;
  discountAmount: number;
  totalAmount: number;
}
