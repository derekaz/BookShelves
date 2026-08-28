# Slice B — Blazor UI Integration (P1)

## Objective
Add Web UI capabilities for per-user book action tracking using Slice A APIs and security model.

## Scope
- Add book-level UI actions in shared Blazor components:
  - Add to To-Be-Read
  - Log pages read
  - Mark finished
- Add action history/progress display on book detail pages.
- Use Slice A web endpoints/services without introducing a parallel data flow.

## Dependencies
- Slice A completed and stable:
  - contracts + validation
  - Datasync table + access control
  - web server service endpoints
  - authorization and tests

## Acceptance Summary
- Authenticated users can record/view their own book actions from UI.
- Admin behavior remains policy-driven and consistent.
- Books/authors remain globally visible per existing behavior.
- Existing sync/process conventions remain unchanged.

## Notes
This slice is intentionally UI-focused; avoid expanding into MAUI-specific implementation in this phase.
