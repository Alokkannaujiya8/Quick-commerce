import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule],
  template: `
    <footer class="footer">
      <div class="footer-container">
        <div class="footer-col">
          <div class="footer-logo">
            <span class="logo-highlight">Quick</span>Cart
          </div>
          <p class="tagline">Superfast groceries and daily essentials delivered to your doorstep in 10-15 minutes.</p>
          <div class="badges">
            <span class="badge">🚀 10-Min Delivery</span>
            <span class="badge">🔒 Secure Payments</span>
            <span class="badge">💯 Freshness Guaranteed</span>
          </div>
        </div>

        <div class="footer-col">
          <h4>Popular Categories</h4>
          <ul>
            <li>Fresh Vegetables & Fruits</li>
            <li>Dairy, Bread & Eggs</li>
            <li>Cold Drinks & Juices</li>
            <li>Instant Food & Snacks</li>
            <li>Personal & Home Care</li>
          </ul>
        </div>

        <div class="footer-col">
          <h4>Customer Service</h4>
          <ul>
            <li>Help & Support</li>
            <li>Return Policy</li>
            <li>Terms & Conditions</li>
            <li>Privacy Policy</li>
            <li>Track Order</li>
          </ul>
        </div>

        <div class="footer-col">
          <h4>Download App</h4>
          <p>Get the best quick-commerce experience on mobile.</p>
          <div class="app-buttons">
            <button class="store-btn">📱 App Store</button>
            <button class="store-btn">🤖 Google Play</button>
          </div>
        </div>
      </div>

      <div class="copyright">
        © 2026 QuickCart E-Commerce. All rights reserved. Built with .NET 10 & Angular 20.
      </div>
    </footer>
  `,
  styles: [`
    .footer {
      background: #111827;
      color: #9ca3af;
      padding: 3rem 1.5rem 1.5rem;
      margin-top: 4rem;
    }
    .footer-container {
      max-width: 1280px;
      margin: 0 auto;
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 2.5rem;
    }
    .footer-logo {
      font-size: 1.5rem;
      font-weight: 800;
      color: #ffffff;
      margin-bottom: 0.75rem;
    }
    .logo-highlight {
      color: #10b981;
    }
    .tagline {
      font-size: 0.85rem;
      line-height: 1.5;
      margin-bottom: 1.25rem;
    }
    .badges {
      display: flex;
      flex-direction: column;
      gap: 0.4rem;
    }
    .badge {
      font-size: 0.75rem;
      background: #1f2937;
      color: #d1d5db;
      padding: 0.25rem 0.5rem;
      border-radius: 6px;
      width: fit-content;
    }
    h4 {
      color: #ffffff;
      font-size: 1rem;
      font-weight: 700;
      margin-bottom: 1rem;
    }
    ul {
      list-style: none;
      padding: 0;
      margin: 0;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      font-size: 0.85rem;
    }
    li:hover {
      color: #10b981;
      cursor: pointer;
    }
    .app-buttons {
      display: flex;
      gap: 0.5rem;
      margin-top: 0.75rem;
    }
    .store-btn {
      background: #1f2937;
      color: #ffffff;
      border: 1px solid #374151;
      padding: 0.5rem 0.75rem;
      border-radius: 8px;
      cursor: pointer;
      font-size: 0.8rem;
    }
    .copyright {
      max-width: 1280px;
      margin: 2.5rem auto 0;
      padding-top: 1.5rem;
      border-top: 1px solid #1f2937;
      text-align: center;
      font-size: 0.8rem;
    }
  `]
})
export class FooterComponent {}
