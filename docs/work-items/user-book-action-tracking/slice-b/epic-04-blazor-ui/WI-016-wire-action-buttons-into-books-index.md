# WI-016: Wire Action Buttons into Books Index for Authenticated Users

## Epic
Epic 4 — Blazor UI

## Priority
P1

## Status
Pending

## Goal
Add per-row action buttons to the Books index page (`/books`) that allow authenticated users to record book actions directly from the book list, using the dialogs from WI-014.

## Scope
- Update `src/BookShelves.Shared/Components/Pages/Books/Index.razor`:
  - Add a new "My Actions" column (or extend the existing Actions column) visible to authenticated users.
  - Wrap new action buttons in `<AuthorizeView Policy="Authenticated">` so anonymous and unauthenticated users see no action controls.
  - Add three icon buttons per row:
	- "Add to To-Be-Read" — opens `AddToBeReadDialog` passing `BookId` and current `UserId`.
	- "Log Pages Read" — opens `LogPagesReadDialog` passing `BookId` and current `UserId`.
	- "Mark Finished" — opens `MarkFinishedDialog` passing `BookId` and current `UserId`.
  - Inject `IDialogService` and `AuthenticationStateProvider` to support dialog launch and user ID resolution.
  - After a dialog is submitted successfully, optionally show a brief `ISnackbar` confirmation.
- Preserve all existing admin-only edit/delete row buttons and their `[Authorize(Roles = "Administrator")]` behavior unchanged.

## Out of Scope
- Changing admin edit/delete buttons.
- Inline editing of actions from this page.
- My Books page changes (covered in WI-015).

## Dependencies
- WI-014 (dialog components must exist).
- WI-013 (confirms `/my-books` route context and shared imports are stable).

## Acceptance Criteria
- Authenticated non-admin users see action buttons on every book row and can open each dialog.
- Admin users see both admin row buttons and the new action buttons.
- Anonymous users see neither the new action buttons nor the existing admin buttons.
- Submitting a dialog from the books index records the action and shows feedback.
- Existing admin book management workflow is unaffected.

## Notes
Resolve `UserId` from `AuthenticationStateProvider` at the page level and pass down to dialogs as a parameter, consistent with user isolation requirements.
Use icon buttons to keep the row compact; follow the existing `MudIconButton` style used for Edit/Delete.
