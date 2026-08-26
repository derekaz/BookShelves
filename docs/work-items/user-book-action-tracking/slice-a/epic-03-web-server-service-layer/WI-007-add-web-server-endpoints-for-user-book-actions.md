# WI-007: Add Web Server Endpoints for User Book Actions

## Epic
Epic 3 — Web Server Service Layer

## Priority
P0

## Status
Not Started

## Goal
Expose web server endpoints for user-book actions, consistent with existing `/booksdata` and `/authorsdata` patterns.

## Scope
- Update `src/BookShelves.Web/BookShelves.Web/Program.cs` with endpoints such as:
  - `GET /bookuseractionsdata`
  - `POST /bookuseractionsdata`
  - optional `PUT/DELETE` for MVP completeness
- Require authorization on all endpoints.
- Use new `BookUserActionsDataService` for endpoint operations.

## Out of Scope
- UI rendering behavior.
- Admin reporting endpoints.

## Dependencies
- WI-006
- WI-010

## Acceptance Criteria
- Endpoints are routable and protected.
- Endpoint responses follow existing project conventions for success/failure handling.
- Non-admin callers only access their own action data via downstream API enforcement.

## Notes
Keep endpoint style consistent with current minimal API route and exception handling patterns.