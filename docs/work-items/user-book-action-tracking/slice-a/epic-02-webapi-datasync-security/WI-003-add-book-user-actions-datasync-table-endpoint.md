# WI-003: Add BookUserActions Datasync Table Endpoint

## Epic
Epic 2 — WebApi Datasync Table + Security

## Priority
P0

## Status
Not Started

## Goal
Expose user-book actions through a Datasync table controller matching existing table endpoint patterns and the updated parent/details action-record contract.

## Scope
- Add `BookUserActionsController : TableController<BookUserAction>` in `src/BookShelves.WebApi/Controllers`.
- Use route pattern: `/tables/BookUserActions`.
- Configure controller options/logging consistent with `BooksController` and `AuthorsController`.
- Require authenticated access.
- Keep the controller aligned with the `StartTimeUtc`, `EndTimeUtc`, `ActionType`, and typed `Details` payload defined in WI-001/WI-002.

## Out of Scope
- Non-table convenience endpoints.
- Blazor page updates.

## Dependencies
- WI-001
- WI-002

## Acceptance Criteria
- Endpoint is discoverable and routable at `/tables/BookUserActions`.
- Unauthorized requests are rejected.
- CRUD operations use repository contract like existing Datasync table controllers.

## Notes
Keep implementation parallel to existing table controllers to preserve familiar behavior.