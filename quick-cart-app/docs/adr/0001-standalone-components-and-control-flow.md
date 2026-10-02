# ADR 0001: Adoption of Standalone Components & Modern Built-in Control Flow

## Status
**Accepted**

## Context
Angular historically required `NgModule` declarations to assemble components, directives, pipes, and providers. Additionally, templating relied on structural directives (`*ngIf`, `*ngFor`, `*ngSwitch`) imported from `CommonModule`. With modern Angular releases (v17 through v20), standalone components became the standard paradigm, and native template control flow syntax (`@if`, `@for`, `@switch`) was introduced into the compiler.

## Decision
1. All components across QuickCart are declared as standalone components (`standalone: true` or default). No `NgModule` classes exist in the repository.
2. All templates must exclusively use modern control flow syntax (`@if`, `@else`, `@for`, `@switch`).
3. Every `@for` loop must provide a track expression (`track item.id` or `track $index`).

## Consequences
### Positive
* Eliminated `NgModule` boilerplate and circular module dependencies.
* Finer-grained tree-shaking and smaller initial bundle sizes (~98 kB transfer).
* Cleaner template syntax without requiring `CommonModule` imports for basic flow.
* Up to 30-40% faster template type-checking and runtime rendering.

### Negative / Trade-offs
* Developers familiar only with AngularJS or older Angular versions must adapt to modern control flow semantics.
