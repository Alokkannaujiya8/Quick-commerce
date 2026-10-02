# Task: Attach authGuard to Checkout Route

* **Task ID**: TASK-001
* **Priority**: Medium
* **Target File**: `src/app/app.routes.ts`

## Objective
Enforce that customers must be authenticated before accessing the checkout and payment pages.

## Verification Checklist
1. Import `authGuard` from `./core/guards/auth.guard`.
2. Add `canActivate: [authGuard]` to the `/checkout` route definition.
3. Verify unauthenticated visits redirect to `/login`.
4. Run `npm test` and `npm run build`.
