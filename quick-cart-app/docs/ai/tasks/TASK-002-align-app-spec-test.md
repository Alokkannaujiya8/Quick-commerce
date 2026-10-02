# Task: Align app.spec.ts with Shell Template

* **Task ID**: TASK-002
* **Priority**: Low
* **Target File**: `src/app/app.spec.ts`

## Objective
Update the legacy scaffolding test assertion in `src/app/app.spec.ts` which expects an `h1` element containing `'Hello, ecommerce-angular'` so that the test suite achieves 100% pass rate.

## Verification Checklist
1. Review `src/app/app.spec.ts` and `src/app/app.html`.
2. Replace the title query test with a test verifying that `<router-outlet />` is present.
3. Run `npm test -- --watch=false --browsers=ChromeHeadless` and confirm 13 of 13 tests pass.
