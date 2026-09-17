import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  readonly isConnected = signal(false);
  readonly latestOrderStatus = signal<string | null>(null);

  connect(orderId: string): void {
    console.log(`Connecting to live tracking for order: ${orderId}`);
    this.isConnected.set(true);
  }

  disconnect(): void {
    this.isConnected.set(false);
  }
}
