# WI-013: Add My Books Page and Nav Entry for Authenticated Users

## Epic
Epic 4 — Blazor UI

## Priority
P1

## Status
Pending

## Goal
Create a user-facing "My Books" page at `/my-books` for authenticated users to see their book action history, and add a nav link visible only to authenticated users.

## Scope
- Add `src/BookShelves.Shared/Components/Pages/MyBooks/Index.razor`:
  - Route: `@page "/my-books"`
  - Authorize with `Policy="Authenticated"` (same pattern as the weather page in `NavMenu.razor`).
  - Initially render a loading indicator and page title as the shell for WI-015 content.
- Add nav link in `src/BookShelves.Shared/Components/Layout/NavMenu.razor`:
  - Wrap in `<AuthorizeView Policy="Authenticated">` block.
  - Icon: `Icons.Material.Filled.Bookmarks` (or similar appropriate icon).
  - Label: "My Books".
  - Href: `/my-books`.
  - Place after the existing "User Profile" nav link.

## Out of Scope
- Actual data loading or action display (covered in WI-015).
- Dialog components (covered in WI-014).

## Dependencies
- Slice A WI-007 (web server endpoints stable; service registered in DI).

## Acceptance Criteria
- `/my-books` route renders for authenticated users.
- Unauthenticated users are redirected per existing app auth behavior.
- Nav link appears for authenticated users and is hidden from anonymous users.
- Existing nav structure and spacing is unchanged.

## Notes
Follow the `Books/Index.razor` and `NavMenu.razor` patterns for page scaffold and nav link placement.
The page title should read "My Books".
