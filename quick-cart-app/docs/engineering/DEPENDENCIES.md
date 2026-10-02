# QuickCart Frontend — Dependencies Catalog

This document catalogs every production and development dependency installed in `quick-cart-app`, verified directly against `package.json` and `package-lock.json`.

---

## 1. Runtime / Production Dependencies (`dependencies`)

| Package | Verified Version | Purpose | Where Used in Codebase |
| :--- | :--- | :--- | :--- |
| `@angular/core` | `^20.3.0` | Angular framework core: Signals (`signal`, `computed`, `effect`), dependency injection (`inject`), lifecycle hooks (`OnInit`), and standalone decorators (`@Component`, `@Injectable`). | Global throughout all components, services, and `app.config.ts`. |
| `@angular/common` | `^20.3.0` | Common directives, pipes (`number`, `currency`), and document token (`DOCUMENT`). Includes HTTP submodule. | `ProductListComponent`, `CheckoutComponent`, `ThemeService`, `GoogleIdentityService`. |
| `@angular/forms` | `^20.3.0` | Template-driven forms (`FormsModule`, `[(ngModel)]`). | `LoginComponent`, `RegisterComponent`, `OtpVerifyComponent`, `PasswordLoginComponent`, `CheckoutComponent`. |
| `@angular/router` | `^20.3.0` | Application routing, lazy-loading, functional guards (`CanActivateFn`), and navigation directives (`routerLink`, `RouterOutlet`). | `app.routes.ts`, `app.ts`, `auth.guard.ts`, `MainLayoutComponent`, feature components. |
| `@angular/platform-browser` | `^20.3.0` | DOM platform adapter and standalone application bootstrap (`bootstrapApplication`). | `src/main.ts`. |
| `@angular/compiler` | `^20.3.0` | Template and component compilation engine. | Build-time and runtime compilation. |
| `rxjs` | `~7.8.0` | Reactive programming library for handling asynchronous HTTP streams, transformations, and event listeners. | `ApiService`, `AuthService`, `OrderService`, `PaymentService`, `GoogleIdentityService`, `authInterceptor`. |
| `zone.js` | `~0.15.0` | Async task tracking and change detection execution. | `angular.json` polyfill, `app.config.ts` (`provideZoneChangeDetection`). |
| `tslib` | `^2.3.0` | TypeScript runtime helper library containing transpilation helper functions. | Bundled runtime support for TypeScript features. |

---

## 2. Development & Build Dependencies (`devDependencies`)

| Package | Verified Version | Purpose | Where Used |
| :--- | :--- | :--- | :--- |
| `@angular/cli` | `^20.3.8` | Official Angular command-line interface for development, building, and testing. | Terminal tooling (`ng serve`, `ng build`, `ng test`). |
| `@angular/build` | `^20.3.8` | Next-generation application builder powered by Vite and esbuild (`@angular/build:application`, `@angular/build:karma`). | Configured in `angular.json` under `architect.build`, `serve`, and `test`. |
| `@angular/compiler-cli` | `^20.3.0` | Ahead-of-Time (AOT) compiler CLI. | TypeScript compilation and template type-checking. |
| `typescript` | `~5.9.2` | Static type checker and language compiler. | Defined in `tsconfig.json`, `tsconfig.app.json`, `tsconfig.spec.json`. |
| `jasmine-core` | `~5.9.0` | Behavior-driven development testing framework for unit tests. | Unit test execution (`describe`, `it`, `expect`). |
| `@types/jasmine` | `~5.1.0` | TypeScript definitions for Jasmine test framework. | Included in `tsconfig.spec.json`. |
| `karma` | `~6.4.0` | Test runner for executing Jasmine specifications in real or headless browser environments. | Configured under `architect.test` in `angular.json`. |
| `karma-chrome-launcher` | `~3.2.0` | Karma plugin to launch Google Chrome and ChromeHeadless. | `npm test` script. |
| `karma-coverage` | `~2.2.0` | Karma plugin for generating test code coverage metrics. | Test coverage reporter. |
| `karma-jasmine` | `~5.1.0` | Adapter to execute Jasmine tests inside Karma runner. | Test execution harness. |
| `karma-jasmine-html-reporter` | `~2.1.0` | HTML test result reporter for browser test visualization. | Karma interactive runner. |

---

## 3. Notable Architectural Absences & Decisions

* **No State Management Libraries**: Neither `@ngrx/store` nor `@ngxs/store` nor `pinia` is installed. State management is handled completely via native **Angular 20 Signals** and **RxJS**.
* **No CSS/UI Component Frameworks**: Neither `@angular/material` nor `bootstrap` nor `tailwindcss` is installed. Components use custom BEM-like SCSS styles backed by CSS variable design tokens.
* **No Icon Libraries**: Icons are implemented via native Unicode emojis and custom SVGs (e.g. `BrandLogoComponent`).
