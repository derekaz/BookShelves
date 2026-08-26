# WI-012: Add Web Service/Factory/Endpoint Tests

## Epic
Epic 6 — Tests

## Priority
P0

## Status
Not Started

## Goal
Add test coverage for new Web server-side datasync factory, data service, and endpoint wiring for user-book actions.

## Scope
- Add tests under `test/BookShelves.Web.Tests` for:
  - `BookUserActionsDatasyncClientFactory` base URL and endpoint composition behavior
  - `BookUserActionsDataService` token/error handling parity with existing services
  - `/bookuseractionsdata` endpoint auth behavior and basic execution flow
- Reuse patterns from existing datasync factory and server service tests.

## Out of Scope
- End-to-end browser UI tests.
- MAUI sync tests.

## Dependencies
- WI-006
- WI-007
- WI-010

## Acceptance Criteria
- New tests pass and validate expected Web service-layer behavior.
- Existing Web test suites remain green.
- Tests enforce consistency with established server-side data access patterns.

## Notes
Keep tests focused on behavior, not implementation details, to avoid fragile assertions.