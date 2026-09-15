# Slice B — Blazor UI Integration (P1)

## Objective
Add Web UI capabilities for per-user book action tracking using Slice A APIs and security model.

## Scope
- Add book-level UI actions in shared Blazor components:
  - Add to To-Be-Read
  - Log pages read
  - Mark finished
- Add action history/progress display on a dedicated My Books page.
- Add nav entry for authenticated users to access their book actions.
- Wire action buttons into the Books index for authenticated users.
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

---

## Work Item Index

### Epic 4 — Blazor UI
- [Epic 4 Task List](./epic-04-blazor-ui/epic-04-task-list.md)
- [WI-013 - Add My Books page and nav entry for authenticated users](./epic-04-blazor-ui/WI-013-add-my-books-page-and-nav.md)
- [WI-014 - Add book action capture dialogs (To-Be-Read, Pages Read, Finished)](./epic-04-blazor-ui/WI-014-add-book-action-capture-dialogs.md)
- [WI-015 - Add book action history display on My Books page](./epic-04-blazor-ui/WI-015-add-book-action-history-display.md)
- [WI-016 - Wire action buttons into Books index for authenticated users](./epic-04-blazor-ui/WI-016-wire-action-buttons-into-books-index.md)

### Epic 6 — Tests (UI)
- [WI-017 - Add Shared UI component and interaction tests](./epic-04-blazor-ui/WI-017-add-shared-ui-component-tests.md)

## Execution Guide
- [Execution Order and Dependencies](./00-execution-order-and-dependencies.md)

## Shared Constraints
- Use `IBookUserActionsDataService` — no new service abstractions.
- Keep `Books` and `Authors` global/shared.
- Enforce user isolation in UI: only show the current user's own actions.
- Authenticated users see action UI; anonymous users do not.
- Follow existing MudBlazor component and page patterns.
