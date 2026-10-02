# QuickCart Frontend — Angular 20 Reactive Web Application

The `quick-cart-app` is the customer-facing web application for the **QuickCart** 10-minute grocery delivery platform. It is engineered with modern **Angular 20**, featuring standalone components, native control flow, and hybrid reactivity combining **Angular Signals** for state with **RxJS 7.8** for asynchronous event streams.

---

## 1. Architectural Highlights

* **Framework**: Angular 20 (`@angular/core` `^20.3.0`)
* **Language**: TypeScript 5.9 (`strict: true`, `target: ES2022`)
* **Paradigm**: Standalone Components (`standalone: true`) and modern control flow (`@if`, `@for`, `@switch`)
* **State Management**: Reactive state stores using Angular Signals (`signal`, `computed`, `effect`)
* **Styling**: SCSS with design token custom properties and real-time dark/light theme switching via `ThemeService`
* **Authentication**: Multi-channel auth supporting Phone OTP, Email/Password, and Google Identity Services (GIS)

---

## 2. Directory Taxonomy

```text
quick-cart-app/
├── AGENTS.md                  # Operating directives and conventions for AI agents
├── ARCHITECTURE.md            # Comprehensive frontend architecture documentation
├── README.md                  # This file
├── angular.json               # Angular CLI workspace configuration
├── package.json               # Project dependencies and script runner commands
├── tsconfig.json              # TypeScript root configuration
├── docs/                      # Enterprise frontend documentation suite
│   ├── adr/                   # Architectural Decision Records (ADRs 0001–0004)
│   ├── ai/                    # Engineering memory, workflows, and task tracking
│   ├── architecture/          # System specs, component guides, and Mermaid diagrams
│   └── engineering/           # Coding rules, security, performance, and state guidelines
└── src/
    ├── app/
    │   ├── core/              # Singleton services, auth guards, and HTTP interceptors
    │   ├── layout/            # Shell containers (MainLayout, Header, Footer)
    │   ├── modules/           # Autonomous domain feature modules
    │   │   ├── cart/          # Shopping cart state and slide-out drawer
    │   │   ├── catalog/       # Product discovery, filtering, and product cards
    │   │   ├── identity/      # Authentication pages (Login, Register, OTP, Password)
    │   │   ├── inventory/     # Store stock reservation abstractions
    │   │   ├── orders/        # Checkout flow and order confirmation receipt
    │   │   └── payments/      # Payment intent creation and signature verification
    │   ├── shared/            # Reusable UI widgets (UiButton, AlertBanner, AuthCardShell)
    │   ├── app.config.ts      # Core application providers and routing configuration
    │   ├── app.routes.ts      # Lazy-loaded route definitions
    │   └── app.ts             # Root application component
    ├── assets/                # Static assets, brand icons, and images
    ├── environments/          # Environment configuration (development & production)
    ├── index.html             # Host HTML entry point
    └── styles.scss            # Global style tokens and theme variables
```

---

## 3. Prerequisites & Development Setup

* **Node.js**: `v20.x` or higher (verified on `v24.11.0`)
* **npm**: `v10.x` or higher (verified on `11.6.1`)
* **Angular CLI**: `20.3.8` (bundled via `devDependencies`)

### Installation

```powershell
# From the repository root
cd quick-cart-app
npm install
```

### Development Server

```powershell
npm start
# or: npx ng serve
```

Navigate to `http://localhost:4200/`. The application will automatically reload on source changes.

### Production Build

```powershell
npm run build
```

Compiled production artifacts are emitted to `dist/ecommerce-angular/`.

### Running Unit Tests

```powershell
npm test
```

Executes Karma test runner across all `*.spec.ts` test files.

---

## 4. Documentation References

* **Agent Rules**: [AGENTS.md](./AGENTS.md)
* **Frontend Architecture**: [ARCHITECTURE.md](./ARCHITECTURE.md)
* **Engineering Standards**: [docs/engineering/RULES.md](./docs/engineering/RULES.md)
* **Application Structure**: [docs/architecture/application-structure.md](./docs/architecture/application-structure.md)
* **Architectural Decisions**: [docs/adr/README.md](./docs/adr/README.md)

