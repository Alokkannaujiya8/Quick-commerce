# Task: Server-Side Cart Synchronization

* **Task ID**: TASK-003
* **Priority**: High
* **Target Files**: `src/app/modules/cart/services/cart.service.ts`, `src/app/core/services/api.service.ts`

## Objective
Connect `CartService` with the backend Cart module endpoints (`/api/cart/*`) while maintaining client-side Signal reactivity and offline resilience.

## Verification Checklist
1. Review `docs/API_CONTRACTS.md` section on Cart endpoints.
2. Implement backend cart fetch upon customer authentication.
3. Sync cart item additions and deletions to `POST /api/cart/items` and `DELETE /api/cart/items/{id}`.
4. Maintain `localStorage` fallback when unauthenticated.
5. Write unit tests for `CartService` API integration.
