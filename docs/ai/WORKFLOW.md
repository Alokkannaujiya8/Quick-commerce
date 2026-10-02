# AI Development & Operating Workflow

This document defines the 6-phase engineering workflow required for AI coding agents and human engineers executing tasks within the QuickCart backend codebase.

---

## The 6-Phase Engineering Cycle

```text
┌───────────┐     ┌───────────┐     ┌─────────────┐
│ 1. AUDIT  │ ──> │  2. PLAN  │ ──> │ 3. IMPLEMENT│
└───────────┘     └───────────┘     └─────────────┘
                                           │
┌───────────┐     ┌───────────┐            ▼
│ 6. REPORT │ <── │ 5. REVIEW │ <── ┌─────────────┐
└───────────┘     └───────────┘     │   4. TEST   │
                                    └─────────────┘
```

---

## Phase 1: Audit (Inspect First)

**Objective**: Ground all planned work in verified repository reality before modifying any code.

* Check `git status` to identify current branch and existing uncommitted user changes.
* Inspect project files (`.csproj`, `Directory.Build.props`, `global.json`) for referenced packages, versions, and language flags.
* Search existing code under `src/` to verify whether candidate patterns, entities, or services already exist.
* Verify whether the target capability is already implemented or if an existing abstraction can be extended.
* **Rule**: Never assume a dependency, table, or method exists without inspecting the source file.

---

## Phase 2: Plan

**Objective**: Formulate the minimal, safe, architecture-compliant design.

* Define the exact layer boundaries affected:
  * Entities/Invariants in `<Module>.Domain`
  * DTOs/Interfaces in `<Module>.Application`
  * Persistence/Clients in `<Module>.Infrastructure`
  * Controllers/Hubs in `<Module>.Presentation`
* Identify dependencies and inter-module interactions.
* Ensure no cross-schema foreign keys or monolithic couplings are introduced.
* Outline testing plan (unit tests with Moq or integration tests).
* Document task plan in `docs/ai/tasks/` for non-trivial modifications.

---

## Phase 3: Implement

**Objective**: Execute clean, idiomatic, typed C# 14 code adhering to Clean Architecture.

* Maintain nullability compliance (`<Nullable>enable</Nullable>`).
* Use `async`/`await` non-blocking execution throughout, accepting and propagating `CancellationToken`.
* Utilize UTC timestamps (`DateTime.UtcNow`) and leverage `IAuditableEntity` where appropriate.
* Throw domain-specific exceptions (`NotFoundException`, `ValidationException`, `ConflictException`) to leverage `GlobalExceptionHandler`.
* **Prohibitions**:
  * Never alter frontend files in `quick-cart-app/` during backend tasks.
  * Never hardcode connection strings or secrets.
  * Never bypass server-side validation or price calculation.

---

## Phase 4: Test

**Objective**: Prove correctness through compilation and automated test suites.

* Execute solution-wide build verification:
  ```powershell
  dotnet build QuickCart.sln
  ```
  Ensure **0 Warnings** and **0 Errors**.
* Run automated test suites:
  ```powershell
  dotnet test QuickCart.sln
  ```
  Ensure all active tests pass (minimum 55 tests passing across `Identity.Tests` and `Payment.Tests`).
* Write unit tests for newly implemented application services or domain aggregates using xUnit and Moq.

---

## Phase 5: Review

**Objective**: Safety check, boundary verification, and diff auditing.

* Review `git status` and `git diff` to ensure:
  1. No unintended files were created or modified.
  2. No user changes were reverted or corrupted.
  3. No destructive Git commands were run.
* Validate that all links across documentation files are accurate.
* Verify that new migrations (if any) are properly documented in `docs/DATABASE.md`.

---

## Phase 6: Report

**Objective**: Return an authoritative, verifiable summary to the user.

* Report repository baseline (branch, HEAD, working tree state).
* List files created, modified, or verified.
* Present exact test execution results (pass/fail/duration).
* Explicitly report any unconfirmed or unimplemented architectural topics.

