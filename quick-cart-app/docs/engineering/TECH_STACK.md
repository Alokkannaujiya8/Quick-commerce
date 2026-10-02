# QuickCart Frontend — Technology Stack Baseline

This document specifies the verified technology stack, compiler tools, runtime versions, and frontend libraries powering the **QuickCart** client application.

---

## 1. Verified Core Runtime & Tooling

| Component | Verified Version | Verification Source | Notes |
| :--- | :--- | :--- | :--- |
| **Node.js** | `v24.11.0` | CLI verification (`node -v`) | Host JavaScript runtime environment |
| **npm** | `11.6.1` | CLI verification (`npm -v`) | Package manager enforced via `angular.json` (`cli.packageManager: "npm"`) |
| **Angular Framework** | `20.3.0` (`^20.3.0`) | `package.json` | Modern standalone web framework |
| **Angular CLI** | `20.3.8` (`^20.3.8`) | `package.json` | Scaffolding, building, and test orchestration |
| **TypeScript** | `5.9.2` (`~5.9.2`) | `package.json` | Type-safe superset of JavaScript |

---

## 2. Angular Core Packages

All core Angular packages are verified at version `^20.3.0` in `package.json`:

* `@angular/core`: Angular 20 core runtime, Signals API (`signal`, `computed`, `effect`), and dependency injection.
* `@angular/common`: Common directives, pipes, document token, and `@angular/common/http`.
* `@angular/forms`: Template-driven forms (`FormsModule`, `[(ngModel)]`).
* `@angular/router`: Client-side routing with functional guards and lazy-loading support.
* `@angular/platform-browser`: Browser bootstrapping runtime (`bootstrapApplication`).
* `@angular/compiler`: Ahead-of-Time (AOT) and Just-in-Time (JIT) template compiler.
* `@angular/compiler-cli`: Build-time compiler tools.

---

## 3. Asynchronous & Change Detection Infrastructure

* **RxJS**: `~7.8.0` (`package.json`)
  * Used for HTTP client streams, operator chaining (`map`, `tap`, `switchMap`, `catchError`), and event pipelines.
* **Zone.js**: `~0.15.0` (`package.json`, `angular.json`)
  * Configured via `provideZoneChangeDetection({ eventCoalescing: true })` in `app.config.ts`.
  * Event coalescing batches microtasks to avoid redundant change detection passes.
* **tslib**: `^2.3.0` (`package.json`)
  * Runtime library for TypeScript helper functions.

---

## 4. Build, Bundling & Styling System

* **Application Builder**: `@angular/build:application` (`^20.3.8` in `angular.json`).
  * Utilizes **esbuild** for fast Ahead-of-Time compilation and JavaScript bundle generation.
  * Utilizes **Vite** for rapid local development server (`ng serve`).
* **Styles**: SCSS (`inlineStyleLanguage: "scss"`, `src/styles.scss`).
  * Custom CSS variables for light/dark theme design tokens.
  * No heavy external CSS frameworks (Tailwind, Bootstrap) are installed.
* **Asset Pipeline**:
  * Static assets served from `public/` directory (`angular.json`).

---

## 5. UI & Component Architecture

* **UI Libraries**:
  * **Status**: Custom In-House Components.
  * **Evidence**: Verified absence of `@angular/material`, `primeng`, `bootstrap`, or `@tailwind/base` in `package.json`.
  * **Implementation**: Reusable UI components are created under `src/app/shared/components/` (`UiButtonComponent`, `AlertBannerComponent`, `AuthCardShellComponent`, `BrandLogoComponent`, `ThemeToggleComponent`).

---

## 6. Testing Tooling

* **Test Framework**: Jasmine Core `~5.9.0` with `@types/jasmine ~5.1.0`.
* **Test Runner**: Karma `~6.4.0` (`@angular/build:karma`).
* **Karma Plugins**:
  * `karma-chrome-launcher ~3.2.0` (executes headless Chrome)
  * `karma-jasmine ~5.1.0`
  * `karma-jasmine-html-reporter ~2.1.0`
  * `karma-coverage ~2.2.0`
* **Test Polyfills**: `zone.js`, `zone.js/testing` (`angular.json`).
