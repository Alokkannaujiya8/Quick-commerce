import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BrandLogoComponent } from '../brand-logo/brand-logo.component';
import { ThemeToggleComponent } from '../theme-toggle/theme-toggle.component';

@Component({
  selector: 'app-auth-card-shell',
  standalone: true,
  imports: [RouterLink, BrandLogoComponent, ThemeToggleComponent],
  templateUrl: './auth-card-shell.component.html',
  styleUrl: './auth-card-shell.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AuthCardShellComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string>('');
  readonly backLink = input<string>('/');
  readonly backAriaLabel = input<string>('Go back');
  readonly showLegalFooter = input<boolean>(true);
}
