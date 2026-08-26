# WI-005: Register Cosmos Table and Repository

## Epic
Epic 2 — WebApi Datasync Table + Security

## Priority
P0

## Status
Not Started

## Goal
Register `BookUserAction` persistence in WebApi using the existing Cosmos database/container and repository patterns.

## Scope
- Update `src/BookShelves.WebApi/Program.cs`:
  - register `ICosmosTableOptions<BookUserAction>`
  - register `IRepository<BookUserAction>` via `CachedCosmosRepository<BookUserAction>` wrapping the Cosmos table repository.
- Align serializer/settings usage with existing entity registrations.

## Out of Scope
- New Cosmos container provisioning.
- Separate storage account/database.

## Dependencies
- WI-001
- WI-003
- WI-004

## Acceptance Criteria
- App startup resolves repository dependencies for `BookUserAction`.
- Table operations persist and read from existing Cosmos shared container configuration.
- No regressions for existing `Book` and `Author` registrations.

## Notes
Keep this strictly additive and consistent with existing service-registration patterns.