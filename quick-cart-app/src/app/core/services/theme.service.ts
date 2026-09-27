import { DOCUMENT } from '@angular/common';
import { Injectable, computed, effect, inject, signal } from '@angular/core';

export type AppTheme = 'light' | 'dark';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private readonly document = inject(DOCUMENT);
  private readonly storageKey = 'qc_theme';

  readonly theme = signal<AppTheme>(this.getInitialTheme());
  readonly isDark = computed(() => this.theme() === 'dark');

  constructor() {
    effect(() => {
      const currentTheme = this.theme();
      this.applyThemeToDom(currentTheme);
    });
  }

  toggleTheme(): void {
    this.theme.update((current) => (current === 'light' ? 'dark' : 'light'));
  }

  setTheme(newTheme: AppTheme): void {
    this.theme.set(newTheme);
  }

  private getInitialTheme(): AppTheme {
    try {
      const saved = localStorage.getItem(this.storageKey) as AppTheme | null;
      if (saved === 'dark' || saved === 'light') {
        return saved;
      }
      if (
        typeof window !== 'undefined' &&
        window.matchMedia?.('(prefers-color-scheme: dark)').matches
      ) {
        return 'dark';
      }
    } catch {
      // Ignore storage access errors
    }
    return 'light';
  }

  private applyThemeToDom(theme: AppTheme): void {
    const htmlEl = this.document.documentElement;
    const bodyEl = this.document.body;

    if (htmlEl) {
      htmlEl.setAttribute('data-theme', theme);
      htmlEl.classList.toggle('dark-theme', theme === 'dark');
    }
    if (bodyEl) {
      bodyEl.setAttribute('data-theme', theme);
      bodyEl.classList.toggle('dark-theme', theme === 'dark');
    }

    try {
      localStorage.setItem(this.storageKey, theme);
    } catch {
      // Ignore storage write errors
    }
  }
}
