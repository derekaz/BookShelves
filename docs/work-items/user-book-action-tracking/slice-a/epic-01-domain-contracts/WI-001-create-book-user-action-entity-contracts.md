# WI-001: Create BookUserAction Entity Contracts

## Epic
Epic 1 — Domain + Contracts

## Priority
P0

## Status
Not Started

## Goal
Create the core user-book action entity/DTO/view-model contracts, aligned to existing Datasync conventions.

## Scope
- Add `BookUserAction` server model in `src/BookShelves.WebApi`.
- Add shared DTO in `src/BookShelves.Web.Shared/Data`.
- Add UI/service view model in `src/BookShelves.Shared/Presentation/ViewModels`.
- Include fields:
  - `Id`
  - `BookId`
  - `UserId`
  - `ActionType`
  - `OccurredAtUtc`
  - optional `PagesRead`, `Notes`
  - Datasync metadata fields already used in project (`UpdatedAt`, `Version`, `Deleted`, etc.).

## Out of Scope
- Blazor UI changes.
- MAUI local/offline implementation.

## Dependencies
None.

## Acceptance Criteria
- New contracts compile in all touched projects.
- Field names/types are consistent across WebApi/Web.Shared/Shared layers.
- Contract design supports user-scoped filtering and append-only action history.
- No changes to global `Book`/`Author` domain models except necessary references.

## Notes
Follow existing project naming and folder conventions for author/book entities and DTOs.