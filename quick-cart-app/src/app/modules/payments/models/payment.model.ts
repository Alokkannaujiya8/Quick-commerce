export type PaymentGatewayStatus =
  | 'Pending'
  | 'Initiated'
  | 'Authorized'
  | 'Captured'
  | 'Failed'
  | 'Refunded'
  | 'PartiallyRefunded'
  | 'Cancelled';

export interface CreatePaymentIntentRequest {
  orderId: string;
  paymentMethod: string;
  idempotencyKey?: string;
}

export interface PaymentIntentResponse {
  paymentId: string;
  orderId: string;
  amount: number;
  currency: string;
  paymentMethod: string;
  status: PaymentGatewayStatus;
  providerName: string;
  providerOrderId: string;
  providerKeyId: string;
  devVerificationSignature?: string | null;
  isIdempotentReplay: boolean;
  createdAt: string;
}

export interface VerifyPaymentRequest {
  paymentId: string;
  providerOrderId: string;
  providerPaymentId: string;
  providerSignature: string;
}

export interface PaymentAuditLogDto {
  id: string;
  eventType: string;
  previousStatus: string;
  newStatus: string;
  providerReference?: string | null;
  notes?: string | null;
  createdAt: string;
}

export interface PaymentDto {
  id: string;
  orderId: string;
  userId: string;
  amount: number;
  refundedAmount: number;
  currency: string;
  paymentMethod: string;
  status: PaymentGatewayStatus;
  providerName: string;
  providerOrderId: string;
  providerPaymentId?: string | null;
  failureReason?: string | null;
  retryCount: number;
  createdAt: string;
  completedAt?: string | null;
  auditLogs: PaymentAuditLogDto[];
}
