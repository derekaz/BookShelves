# Slice A Execution Order and Dependencies

This sequence is optimized to reduce rework and make security constraints enforceable early.

## Recommended Execution Order

1. **WI-001** — Create BookUserAction entity contracts  
   - Foundation for all downstream API/service/test work.

2. **WI-002** — Define action types and validation rules  
   - Locks the allowed payload shape before endpoint implementation.

3. **WI-003** — Add BookUserActions Datasync table endpoint  
   - Adds route/controller shell using contracts.

4. **WI-004** — Add user-scoped access control provider  
   - Enforces per-user isolation and admin override behavior.

5. **WI-005** — Register Cosmos table and repository  
   - Connects table/controller to existing Cosmos shared container via existing repository pattern.

6. **WI-010** — Formalize admin policy usage for user-scoped tracking  
   - Ensures consistent admin/non-admin behavior and prevents policy drift.

7. **WI-006** — Add server Datasync client factory and data service  
   - Enables Web server-side consumption of new API table.

8. **WI-007** — Add web server endpoints for user book actions  
   - Exposes app-facing web endpoints after service layer is ready.

9. **WI-011** — Add WebApi user-scope authorization tests  
   - Validates isolation/security at API layer.

10. **WI-012** — Add Web service/factory/endpoint tests  
	- Validates Web server wiring and behavior parity.

---

## Dependency Map

- **WI-001**: no dependencies
- **WI-002**: depends on WI-001
- **WI-003**: depends on WI-001, WI-002
- **WI-004**: depends on WI-001, WI-003
- **WI-005**: depends on WI-001, WI-003, WI-004
- **WI-010**: depends on WI-004
- **WI-006**: depends on WI-001, WI-003, WI-005
- **WI-007**: depends on WI-006, WI-010
- **WI-011**: depends on WI-003, WI-004, WI-005, WI-010
- **WI-012**: depends on WI-006, WI-007, WI-010

---

## Practical Checkpoints

- **Checkpoint A (API foundation complete):** WI-001 to WI-005 done.
- **Checkpoint B (policy alignment complete):** WI-010 done.
- **Checkpoint C (Web server integration complete):** WI-006 and WI-007 done.
- **Checkpoint D (verification complete):** WI-011 and WI-012 done.

---

## Notes

- Keep Books/Authors behavior unchanged (global/shared).
- User-book actions are private to owner unless caller is admin.
- Follow existing online/offline Datasync conventions throughout.
