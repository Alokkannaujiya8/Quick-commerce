# AI Engineering Operational Workflow

This document defines the strict 8-stage operational workflow that AI agents and engineers must follow when undertaking tasks in the QuickCart frontend repository.

---

## The 8-Stage Execution Cycle

```text
       ┌───────────┐
       │   Audit   │  Inspect current files, configuration, tests, and git state
       └─────┬─────┘
             │
             ▼
       ┌───────────┐
       │Understand │  Comprehend architecture, types, Signals, and API contracts
       └─────┬─────┘
             │
             ▼
       ┌───────────┐
       │   Reuse   │  Identify existing services, models, and shared presenters
       └─────┬─────┘
             │
             ▼
       ┌───────────┐
       │   Plan    │  Formulate minimal, safe, typed change set
       └─────┬─────┘
             │
             ▼
       ┌───────────┐
       │ Implement │  Write clean, idiomatic Angular 20 TypeScript/SCSS
       └─────┬─────┘
             │
             ▼
       ┌───────────┐
       │   Test    │  Execute builds (ng build) and unit test suite (ng test)
       └─────┬─────┘
             │
             ▼
       ┌───────────┐
       │  Review   │  Verify against rules, templates (@if/@for), and contracts
       └─────┬─────┘
             │
             ▼
       ┌───────────┐
       │  Report   │  Document findings, test results, and discrepancies
       └───────────┘
```

---

## Detailed Stage Definitions

### 1. Audit
* Run `git status` to observe uncommitted changes.
* Inspect `package.json`, `angular.json`, and `tsconfig.json`.
* View existing component implementations and spec files.
* Never assume a library or component exists without direct evidence.

### 2. Understand
* Trace the data flow from component signals through services to the backend API.
* Distinguish between verified features and client-side demo fallbacks.
* Identify all error boundaries and ProblemDetails handling.

### 3. Reuse
* Check `src/app/core/services/` for existing HTTP or auth clients.
* Check `src/app/shared/components/` (`UiButtonComponent`, `AlertBannerComponent`, `AuthCardShellComponent`, `BrandLogoComponent`, `ThemeToggleComponent`) before creating new UI elements.
* Reuse models from `src/app/core/models/` and module model files.

### 4. Plan
* Formulate step-by-step changes respecting standalone components and OnPush change detection.
* Ensure no backend files are modified.
* Ensure no package versions or lockfiles are altered unintentionally.

### 5. Implement
* Write clean, fully typed TypeScript using modern control flow (`@if`, `@for`, `@switch`).
* Use `inject(...)` for dependency injection.
* Use Angular Signals for UI/state and RxJS for asynchronous I/O.
* Strictly preserve existing code and uncommitted modifications.

### 6. Test
* Run `npm run build` to verify compilation and production budgets.
* Run `npm test -- --watch=false --browsers=ChromeHeadless` to verify unit test status.
* Never mark a task complete with unverified compilation errors.

### 7. Review
* Verify that no legacy structural directives (`*ngIf`, `*ngFor`) were introduced.
* Verify that all internal documentation links resolve accurately.
* Confirm that no destructive Git actions were executed.

### 8. Report
* Provide concise, structured summary reports including repository baseline, verified architecture, API discrepancies, test outcomes, and Git safety confirmation.
