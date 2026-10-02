# Build & Deployment Specification

This document details the build architecture, compiler toolchain, production budgets, environment configuration swaps, and artifact distribution of the QuickCart frontend.

---

## 1. Build Pipeline & Toolchain

The build toolchain is configured in `angular.json` and executed via the Angular CLI:

* **Builder**: `@angular/build:application` (`^20.3.8`).
  * Replaces legacy Webpack with high-performance **esbuild** for bundling and Ahead-of-Time compilation.
  * Uses **Vite** as the local development server engine for instant HMR (`ng serve`).
* **Polyfills**: `zone.js` for async execution scheduling.
* **Target ECMAScript**: `ES2022` (`tsconfig.json`).
* **Module Resolution**: `bundler` (`tsconfig.json`).

---

## 2. Build Configurations

### 2.1 Production Configuration (`ng build`)
* **Optimization**: Minification, tree-shaking, dead-code elimination, and CSS purging enabled by default.
* **Source Maps**: Disabled for lean payload sizes.
* **Output Hashing**: `all` (hashes all bundle and asset filenames for cache busting).
* **Budgets**:
  ```json
  "budgets": [
    {
      "type": "initial",
      "maximumWarning": "500kB",
      "maximumError": "1MB"
    },
    {
      "type": "anyComponentStyle",
      "maximumWarning": "8kB",
      "maximumError": "16kB"
    }
  ]
  ```
* **Verified Production Output Metrics**:
  * **Initial Transfer Size**: ~98.19 kB (compressed) across `chunk-*.js`, `main.js`, and `polyfills.js`.
  * **Output Directory**: `dist/ecommerce-angular/`.

### 2.2 Development Configuration (`ng build --configuration development`)
* **Optimization**: Disabled for rapid compilation.
* **Source Maps**: Enabled (`sourceMap: true`) for accurate browser debugging.
* **File Replacements**: Swaps production environment config with development:
  ```json
  "fileReplacements": [
    {
      "replace": "src/environments/environment.ts",
      "with": "src/environments/environment.development.ts"
    }
  ]
  ```

---

## 3. Deployment Topology & Static Hosting

The build output in `dist/ecommerce-angular/browser/` (or `dist/ecommerce-angular/`) consists entirely of static HTML, CSS, JavaScript, and asset files.

### Recommended Deployment Hosts
* **Nginx / Caddy**: High-performance reverse proxy and static file server.
* **Cloudflare Pages / AWS S3 + CloudFront / Azure Static Web Apps / Firebase Hosting**.

### Single-Page Application (SPA) Fallback Rule
Because routing is handled client-side via the HTML5 History API (`@angular/router`), all non-file requests must be rewritten to `index.html`:

#### Example Nginx Configuration
```nginx
server {
    listen 80;
    server_name quickcart.local;
    root /usr/share/nginx/html;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location ~* \.(?:ico|css|js|gif|jpe?g|png|woff2?|eot|ttf|svg)$ {
        expires 6M;
        access_log off;
        add_header Cache-Control "public";
    }
}
```
