# Epic 4 — Blazor UI Task List

## Overview
Deliver authenticated user-facing Blazor UI for recording and viewing personal book actions, using the service layer and contracts established in Slice A.

## Work Items

| ID | Title | Status |
|----|-------|--------|
| WI-013 | Add My Books page and nav entry for authenticated users | Completed |
| WI-014 | Add book action capture dialogs (To-Be-Read, Pages Read, Finished) | Completed |
| WI-015 | Add book action history display on My Books page | Completed |
| WI-016 | Wire action buttons into Books index for authenticated users | Completed |
| WI-017 | Add Shared UI component and interaction tests | Completed |

## Constraints
- All components target `src/BookShelves.Shared` (shared across Web and MAUI hosts).
- Use `IBookUserActionsDataService` without new service abstractions.
- Use existing MudBlazor component patterns (`MudTable`, `MudDialog`, `MudStack`, etc.).
- Authenticated (`Policy="Authenticated"`) gate for all action UI; anonymous users must not see action controls.
- No MAUI-specific implementation in this epic.
