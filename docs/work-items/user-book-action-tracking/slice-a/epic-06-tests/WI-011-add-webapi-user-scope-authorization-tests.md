# WI-011: Add WebApi User-Scope Authorization Tests

## Epic
Epic 6 — Tests

## Priority
P0

## Status
Completed

## Goal
Add integration tests validating user isolation and admin override behavior for `BookUserActions` table operations.

## Scope
- Add tests under `test/BookShelves.WebApi.Tests` covering:
  - anonymous requests rejected
  - authenticated non-admin can access own records
  - authenticated non-admin cannot access other users’ records
  - admin can access cross-user records
  - create/update attempts with mismatched user ownership are rejected or normalized per design
  - payload validation for `StartTimeUtc`, `EndTimeUtc`, `ActionType`, and typed `Details` subtypes behaves as documented in Epic 1
- Follow existing `BooksControllerTests`, `AuthorsControllerTests`, and auth test patterns.

## Out of Scope
- Blazor component tests.
- MAUI data tests.

## Dependencies
- WI-003
- WI-004
- WI-005
- WI-010

## Acceptance Criteria
- Tests pass and clearly verify scoped-access rules.
- Failures provide clear diagnostics for authorization regressions.
- Existing WebApi tests remain green.

## Notes
Prefer stable identity claim setup in test auth helpers to represent both non-admin and admin cases.