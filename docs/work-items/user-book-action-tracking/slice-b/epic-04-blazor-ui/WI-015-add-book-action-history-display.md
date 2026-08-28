# WI-015: Add Book Action History Display on My Books Page

## Epic
Epic 4 — Blazor UI

## Priority
P1

## Status
Pending

## Goal
Populate the My Books page (`/my-books`) with the authenticated user's book action history, grouped or sorted by book, with key action metadata visible in the list.

## Scope
- Update `src/BookShelves.Shared/Components/Pages/MyBooks/Index.razor` to:
  - Inject `IBookUserActionsDataService` and `AuthenticationStateProvider`.
  - On `OnInitializedAsync`, load actions via `GetBookUserActionsAsync()`.
  - Display actions in a `MudTable` (or similar) with columns:
	- Book title (derive from BookId for now; lookup or display raw ID if title resolution is not available in this WI).
	- Action type (formatted display name from `BookUserActionTypes`).
	- Date (use `StartTimeUtc` or `EndTimeUtc` as appropriate per action type).
	- Type-specific detail summary (e.g., pages read count, rating, reminder date).
  - Show an empty state message when no actions exist.
  - Show a loading state while fetching.
  - Handle and display errors gracefully (consistent with existing page error handling patterns).
- Do not show other users' actions; the Slice A service layer enforces user scoping server-side.

## Out of Scope
- Editing or deleting existing actions (post-slice scope).
- Book title resolution from a separate books service (acceptable to show BookId initially; a follow-up can enrich).
- Action capture entry points (covered in WI-016).

## Dependencies
- WI-013 (page shell must exist).
- Slice A WI-006 and WI-007 (service layer and endpoints registered and functional).

## Acceptance Criteria
- Authenticated user sees their own action records on `/my-books`.
- Table renders action type, date, and type-specific summary fields correctly.
- Empty state is shown when no actions are present.
- Page handles service errors without unhandled exceptions.
- No other user's records are visible.

## Notes
Keep table design consistent with existing `Books/Index.razor` table patterns.
Use `BookUserActionTypes` constants for display name formatting rather than raw string values.
