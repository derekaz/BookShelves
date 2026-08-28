# WI-014: Add Book Action Capture Dialogs (To-Be-Read, Pages Read, Finished)

## Epic
Epic 4 — Blazor UI

## Priority
P1

## Status
Completed

## Goal
Add MudBlazor dialog components in `src/BookShelves.Shared` for each supported book action type, enabling authenticated users to record actions against a specific book.

## Scope
- Add `src/BookShelves.Shared/Components/Pages/MyBooks/AddToBeReadDialog.razor`:
  - Accept `BookId` and `UserId` as parameters.
  - Fields: optional Notes (`MudTextField`), optional RemindAt date (`MudDatePicker`).
  - On submit: call `IBookUserActionsDataService.CreateBookUserActionAsync` using `BookUserActionViewModel.CreateToBeRead(...)`.
  - Show success/error feedback consistent with existing dialog patterns.
- Add `src/BookShelves.Shared/Components/Pages/MyBooks/LogPagesReadDialog.razor`:
  - Accept `BookId` and `UserId` as parameters.
  - Fields: required PagesRead (`MudNumericField<int>`), optional Notes, optional date range (StartTime/EndTime via `MudDatePicker`).
  - On submit: call `CreateBookUserActionAsync` using `BookUserActionViewModel.CreatePagesRead(...)`.
- Add `src/BookShelves.Shared/Components/Pages/MyBooks/MarkFinishedDialog.razor`:
  - Accept `BookId` and `UserId` as parameters.
  - Fields: optional Rating 1–5 (`MudNumericField<int?>`), optional Notes, optional finish date.
  - On submit: call `CreateBookUserActionAsync` using `BookUserActionViewModel.CreateFinished(...)`.
- Each dialog follows the `MudDialog` / `DialogContent` / `DialogActions` structure used in `BookDetail.razor`.
- Each dialog injects `IBookUserActionsDataService` and `ISnackbar` for feedback.

## Out of Scope
- Launching dialogs from any page (covered in WI-016).
- Display or listing of existing actions (covered in WI-015).

## Dependencies
- WI-013 (page shell; confirms route context and imports are stable).
- Slice A WI-001, WI-002 (contracts and action types locked).
- Slice A WI-007 (service registered in DI).

## Acceptance Criteria
- Each dialog component compiles and renders correctly when opened via `IDialogService`.
- Submitting a valid form calls the data service and closes the dialog on success.
- Validation prevents submission when required fields are missing.
- Error states are communicated without crashing the page.
- Component file structure and naming follow existing conventions.

## Notes
Use `BookUserActionDetailsFactory` for metadata construction if useful.
Follow MudBlazor dialog pattern from `BookDetail.razor` for `MudDialog` structure and button placement.
Inject `IDialogService` callers will use the standard `DialogService.ShowAsync<T>` pattern.
