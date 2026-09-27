import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-alert-banner',
  standalone: true,
  templateUrl: './alert-banner.component.html',
  styleUrl: './alert-banner.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AlertBannerComponent {
  readonly message = input<string | null>(null);
  readonly type = input<'error' | 'info' | 'success' | 'warning'>('info');
}
