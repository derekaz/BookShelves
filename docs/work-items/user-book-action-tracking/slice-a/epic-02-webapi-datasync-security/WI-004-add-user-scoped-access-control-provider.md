# WI-004: Add User-Scoped Access Control Provider

## Epic
Epic 2 — WebApi Datasync Table + Security

## Priority
P0

## Status
Not Started

## Goal
Enforce user isolation for user-book action data while allowing admin cross-user access.

## Scope
- Add `BookUserActionsAccessControlProvider` under `src/BookShelves.WebApi/...DataAccess`.
- Implement user identity resolution from claims using existing patterns.
- `GetDataView()` behavior:
  - anonymous => no rows
  - non-admin => rows where `UserId == currentUserId`
  - admin => all rows
- `IsAuthorizedAsync(...)` behavior:
  - enforce same scope rules for read/create/update/delete.
  - for non-admin create/update, enforce server-side ownership (`UserId = currentUserId`).

## Out of Scope
- New authentication providers.
- Role model redesign.

## Dependencies
- WI-001
- WI-003

## Acceptance Criteria
- Non-admin users cannot view or mutate other users’ records.
- Admin users can perform cross-user operations when authorized.
- Security enforcement exists in provider logic (not client-side only).

## Notes
This is a core security gate for the feature and should be implemented before UI integration.