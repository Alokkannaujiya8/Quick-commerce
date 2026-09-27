import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'app-ui-button',
  standalone: true,
  templateUrl: './ui-button.component.html',
  styleUrl: './ui-button.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UiButtonComponent {
  readonly type = input<'button' | 'submit' | 'reset'>('button');
  readonly variant = input<'primary' | 'outline' | 'soft'>('primary');
  readonly disabled = input<boolean>(false);
  readonly loading = input<boolean>(false);
  readonly loadingText = input<string>('Please wait...');
  readonly fullWidth = input<boolean>(true);

  readonly clicked = output<MouseEvent>();

  onClick(event: MouseEvent): void {
    if (this.disabled() || this.loading()) {
      event.preventDefault();
      return;
    }
    this.clicked.emit(event);
  }
}
