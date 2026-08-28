# Slice C — MAUI Offline Parity (Later)

## Objective
Extend user-book action tracking to MAUI with the same online/offline synchronization model already used in the app.

## Scope
- Add MAUI local model/table wiring for `BookUserAction`.
- Add `SyncDbContext` entity endpoint mapping for actions.
- Add MAUI data service operations for create/read/update/delete as needed.
- Align push/pull conflict and retry behavior with existing sync implementation.
- Add MAUI data tests for local persistence and sync behavior.

## Dependencies
- Slice A complete and stable (API contracts/security/endpoints).
- Slice B behavior finalized enough to mirror user experience where relevant.

## Acceptance Summary
- MAUI can capture action events while offline and sync them reliably.
- User scoping rules remain enforced server-side and respected client-side.
- No divergence from existing sync orchestration patterns.

## Notes
Do not introduce a separate sync pipeline. Reuse existing `SyncDbContext` and unit-of-work/sync services patterns.
