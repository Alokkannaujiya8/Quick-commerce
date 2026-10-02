# QuickCart Frontend — Engineering Rules & Coding Standards

This document establishes the mandatory engineering rules, TypeScript guidelines, Angular standards, and coding conventions enforced in the QuickCart frontend.

---

## 1. TypeScript Coding Rules

* **Strict Mode**: The application is compiled with `"strict": true`, `"noImplicitOverride": true`, `"noPropertyAccessFromIndexSignature": true`, `"noImplicitReturns": true`, and `"noFallthroughCasesInSwitch": true` (`tsconfig.json`).
* **Explicit Typing**:
  * Avoid `any` at all costs. Where unavoidable due to external untyped third-party interfaces (e.g. window Google object before SDK loads), isolate the cast to dedicated services (`GoogleIdentityService`).
  * Always provide return types for public service methods, component helpers, and factory functions.
* **Nullable Reference Handling**: Explicitly account for `null` and `undefined` in interfaces and models (e.g. `User | null`, `string | undefined`).
* **Immutability**:
  * Prefer `readonly` modifiers on injected properties, service signals, and constants.
  * When updating array or object signals, use immutable updates (e.g. `[...this.items(), newItem]` or `.map()`), never in-place mutation.

---

## 2. Angular Component Rules

* **Standalone Architecture**:
  * Every component must declare `standalone: true` (or adhere to Angular 20 standalone defaults).
  * Do NOT create `NgModule` classes.
  * Explicitly specify dependencies in the `imports: [...]` array of the `@Component` decorator.
* **Change Detection Strategy**:
  * All components must declare `changeDetection: ChangeDetectionStrategy.OnPush`.
  * Rely on Angular Signals, `@Input()` bindings, and asynchronous pipes/events to trigger change detection runs.
* **Template Syntax (Modern Control Flow)**:
  * Mandatory: Use `@if`, `@else if`, `@else`, `@for`, and `@switch`.
  * Prohibited: Legacy structural directives (`*ngIf`, `*ngFor`, `*ngSwitch`).
  * In `@for` blocks, the `track` expression is mandatory (e.g. `@for (item of items(); track item.id)`).
* **Component Boundaries & Selectors**:
  * Use the `app-` prefix for all component selectors (enforced in `angular.json`).
  * Separate templates and styles into `.html` and `.scss` files when component logic exceeds trivial single-element presenters.

---

## 3. Service & Dependency Injection Rules

* **Root Singletons**: Services intended for application-wide availability must declare `@Injectable({ providedIn: 'root' })`.
* **Functional Dependency Injection**:
  * Use the `inject(...)` function instead of constructor parameter injection:
    ```typescript
    // Correct
    export class CartService {
      private readonly http = inject(HttpClient);
    }

    // Discouraged
    export class CartService {
      constructor(private http: HttpClient) {}
    }
    ```
* **Separation of Concerns**:
  * Components must not directly make HTTP calls or handle local storage persistence. Delegate data fetching, state caching, and business calculations to services (`ApiService`, `AuthService`, `CartService`, `OrderService`, `PaymentService`).

---

## 4. Reactivity & Async Rules

### Angular Signals
* Use `signal<T>(initialValue)` for state that changes over time within a component or service.
* Use `computed(() => ...)` for pure derived state. Do not trigger side effects inside `computed`.
* Use `effect(() => ...)` exclusively inside injection contexts for side effects such as local storage synchronization or DOM attribute manipulation.
* Protect service signals by exposing them as read-only (`this.currentUser.asReadonly()`).

### RxJS Pipelines
* Use `Observable<T>` for asynchronous event streams and HTTP operations.
* Always clean up long-lived subscriptions:
  * For HTTP requests, Angular's `HttpClient` completes automatically upon response.
  * For continuous streams, use `takeUntilDestroyed()`, `first()`, or async pipe subscriptions.
* Handle stream errors explicitly with `catchError` to avoid breaking outer streams.

---

## 5. Form & Validation Rules

* **Template-Driven Forms**: The application currently uses `FormsModule` and `[(ngModel)]` for lightweight, reactive form state in login, register, and checkout flows.
* **Input Sanitization**:
  * Sanitize phone numbers to 10 digits (`value.replace(/\D/g, '').slice(0, 10)`).
  * Sanitize OTP codes to 6 digits (`value.replace(/\D/g, '').slice(0, 6)`).
* **Validation Feedback**:
  * Disable submission buttons while `isLoading()` is true.
  * Display user-friendly, actionable error messages using `AlertBannerComponent` or inline alert containers.

---

## 6. Accessibility & Styling Conventions

* **Theme Tokens**: All colors and backgrounds must reference CSS variables defined in `src/styles.scss` (e.g. `var(--bg-primary)`, `var(--text-primary)`, `var(--border-color)`).
* **Contrast & Theme Support**: Ensure all text elements meet WCAG AA contrast standards across both `light` and `dark` themes.
* **Semantic HTML**: Use semantic tags (`<header>`, `<main>`, `<footer>`, `<button>`, `<input>`, `<nav>`) rather than arbitrary `<div>` elements.

---

## 7. Git & AI Operating Constraints

* **Strict Non-Destructive Directives**:
  * Never run `git reset`, `git restore`, `git checkout .`, `git clean`, `git stash`, `git rebase`, `git commit`, `git push`.
  * Preserve all user working tree edits.
* **Audit-First**: Always verify project configuration in `package.json`, `angular.json`, and `tsconfig.json` before proposing architectural changes.
