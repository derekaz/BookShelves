# WI-006: Add Server Datasync Client Factory and Data Service

## Epic
Epic 3 — Web Server Service Layer

## Priority
P0

## Status
Not Started

## Goal
Add server-side service plumbing so Web can interact with `BookUserActions` via the same Datasync client model used for books/authors.

## Scope
- Add `BookUserActionsDatasyncClientFactory` in `src/BookShelves.Web/BookShelves.Web/Services`.
- Add `BookUserActionsDataService` in `src/BookShelves.Web/BookShelves.Web/Services/Server`.
- Follow existing patterns for:
  - token handling
  - base URL resolution
  - datasync endpoint composition
  - error handling/logging

## Out of Scope
- Blazor page updates.
- MAUI sync configuration.

## Dependencies
- WI-001
- WI-003
- WI-005

## Acceptance Criteria
- Service resolves from DI and can call Datasync table endpoints.
- Factory uses the same configuration conventions as existing datasync factories.
- Error behavior aligns with existing server data services.

## Notes
Keep naming and code shape parallel to `BooksDataService` and `AuthorsDataService` for maintainability.