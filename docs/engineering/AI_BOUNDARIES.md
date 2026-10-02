# AI Agent Operational Boundaries & Safety Constraints

This document establishes the precise boundaries, permissions, approval gates, and prohibited actions for AI coding agents operating on the QuickCart repository.

---

## 1. Autonomous Modification Permissions (What AI Agents May Modify)

AI coding agents are permitted to autonomously propose, write, and modify:
1. **Application Logic & Use Cases**:
   * Implementing new service interfaces or CQRS handlers under `<Module>.Application`.
   * Adding new validation logic and DTO models.
2. **Repository & Infrastructure Adapters**:
   * Writing concrete repositories, EF Core queries, and external API client implementations under `<Module>.Infrastructure`.
3. **Presentation & Controller Endpoints**:
   * Adding or updating API controllers in `<Module>.Presentation` adhering to existing REST conventions.
4. **Unit & Integration Tests**:
   * Adding test fixtures, mocks, and test assertions in `tests/`.
5. **Documentation**:
   * Creating, updating, and maintaining architecture, engineering, and task documentation under `docs/`.

---

## 2. Operations Requiring Explicit Human Approval

The following actions require explicit user confirmation prior to execution:
1. **Schema & Migration Generation**:
   * Adding new EF Core database migrations or altering entity database mappings.
2. **NuGet Package Addition or Version Upgrades**:
   * Introducing new third-party dependencies or bumping framework versions.
3. **Authentication & Authorization Policy Modifications**:
   * Changing token lifetimes, signing keys, password hashing configurations, or Google OAuth settings.
4. **Payment Gateway Integration Alterations**:
   * Changing payment signature algorithms, webhook verification logic, or refund processing workflows.
5. **Architectural Restructuring**:
   * Adding new domain modules, splitting the modular monolith, or altering shared `BuildingBlocks`.

---

## 3. Strictly Prohibited Operations

AI agents must **NEVER** perform the following actions under any circumstances:
1. **Destructive Git Commands**:
   * Executing `git reset`, `git restore`, `git checkout .`, `git clean`, `git stash`, `git rebase`, `git commit`, or `git push`.
2. **Modifying Frontend Files during Backend Tasks**:
   * Altering Angular files in `quick-cart-app/` unless the prompt specifically demands frontend edits.
3. **Overwriting or Discarding Uncommitted User Work**:
   * Always check `git status` and ensure user modifications are preserved intact.
4. **Manual Schema Alterations**:
   * Directly modifying EF Core migration designer files, snapshot files, or executing raw DDL queries outside of EF Core migrations.
5. **Hardcoding Secrets or Credentials**:
   * Committing API keys, private keys, passwords, or connection strings into code or default configuration files.
6. **Hallucinating Implemented Capabilities**:
   * Documenting unverified architectural patterns as active when they do not exist in code.

---

## 4. Domain-Specific Change Policies

### 4.1 Payment Module Rules
* **Authoritative Server Pricing**: All calculations of order subtotals, delivery fees, and payable totals must be computed or verified server-side. Never trust amounts received from client requests.
* **Idempotency Enforcement**: Every payment mutation must accept and validate an `IdempotencyKey`.
* **Signature Verification**: Signature checks must always use constant-time byte comparisons (`CryptographicOperations.FixedTimeEquals`).

### 4.2 Identity & Auth Module Rules
* **Password Integrity**: Passwords must always pass through `BCrypt.EnhancedHashPassword`.
* **Token Storage**: Refresh tokens must only be stored as SHA-256 hashes (`TokenHash`).
* **Rotation**: Every token refresh must rotate the refresh token.

### 4.3 Database Migration Policy
1. Before creating a migration, inspect existing migrations in `Persistence/Migrations/` to ensure naming and schema alignment.
2. Migrations must be scoped to the specific module infrastructure project.
3. Update `docs/DATABASE.md` immediately upon creating any new migration.

