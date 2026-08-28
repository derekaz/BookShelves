# WI-017: Add Shared UI Component and Interaction Tests

## Epic
Epic 6 — Tests (UI)

## Priority
P1

## Status
Pending

## Goal
Add test coverage for the new Blazor UI components in `src/BookShelves.Shared` introduced in Slice B.

## Scope
- Add tests under `test/BookShelves.Shared.Tests` for:
  - `AddToBeReadDialog` — validates that required/optional fields render, that valid submission calls `CreateBookUserActionAsync` with the expected `ToBeRead` action type, and that the dialog closes on success.
  - `LogPagesReadDialog` — validates required PagesRead field enforcement and correct `PagesRead` action type on submit.
  - `MarkFinishedDialog` — validates optional Rating field and correct `Finished` action type on submit.
  - `MyBooks/Index.razor` — validates that actions returned by the mock `IBookUserActionsDataService` are rendered in the table, that the empty state is shown when no actions exist, and that errors are handled without crashing.
  - `Books/Index.razor` action buttons — validates that action buttons are visible to authenticated users, hidden from anonymous users, and that clicking a button opens the appropriate dialog.
- Use bUnit for component rendering tests; follow existing test patterns in the project.
- Mock `IBookUserActionsDataService` to avoid real service calls.

## Out of Scope
- End-to-end browser tests.
- MAUI-specific tests.
- Admin edit/delete behavior (already covered in existing tests).

## Dependencies
- WI-013
- WI-014
- WI-015
- WI-016

## Acceptance Criteria
- All new tests pass.
- Existing test suites remain green.
- Tests cover both authenticated and unauthenticated rendering paths for auth-gated controls.
- Service mock assertions validate correct `ActionType` and field mapping per action subtype.

## Notes
Follow `docs/Testing-Strategy.md` ownership mapping — shared UI component tests belong in `test/BookShelves.Shared.Tests`.
Reuse any existing bUnit test helpers or auth context setup already present in the test project.
