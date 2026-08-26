# Epic 1 — Domain + Contracts Task List

## Goal
Deliver the core contracts and validation foundation for user book action tracking.

## Dependency Notes
- Complete **WI-001** before **WI-002**.
- Keep the contracts aligned across `BookShelves.WebApi`, `BookShelves.Web.Shared`, and `BookShelves.Shared`.

## Task List

### 1) Confirm existing conventions
- [x] Review existing `Book` and `Author` entity, DTO, and view-model patterns.
- [x] Identify the Datasync metadata fields already used elsewhere in the solution.
- [x] Confirm naming and folder conventions for new contracts.

### 2) Create the server domain model
- [x] Add the `BookUserAction` server model in `src/BookShelves.WebApi`.
- [x] Include the required fields: `Id`, `BookId`, `UserId`, `ActionType`, `StartTimeUtc`, `EndTimeUtc`.
- [x] Include a subtype-friendly `Details` payload for record-specific metadata.
- [x] Add metadata subtypes for supported action types instead of flat optional fields.
- [x] Preserve the existing Datasync metadata shape used by other server entities.

### 3) Create the shared contract
- [x] Add a shared `BookUserAction` DTO in `src/BookShelves.Web.Shared/Data`.
- [x] Keep property names and types aligned with the server model.
- [x] Verify the DTO is suitable for API transport and client consumption.

### 4) Create the UI/service view model
- [x] Add a `BookUserAction` view model in `src/BookShelves.Shared/Presentation/ViewModels`.
- [x] Match the shared DTO fields used by the app UI and service layer.
- [x] Keep the view model ready for future Blazor and MAUI consumption.

### 5) Define supported action types
- [x] Add an action type enum or equivalent constants.
- [x] Include the MVP action set: `ToBeRead`, `PagesRead`, `Finished`.
- [x] Ensure the action type definition is reused across all touched layers.

### 6) Add validation rules
- [x] Require `BookId`.
- [x] Require `ActionType` and reject unsupported values.
- [x] Require `StartTimeUtc` and `EndTimeUtc`.
- [x] Ensure `EndTimeUtc` is not earlier than `StartTimeUtc`.
- [x] Require `Details` and ensure it matches the action subtype.
- [x] Enforce non-negative page counts where applicable.
- [x] Keep validation consistent with the solution's existing API validation behavior.

### 7) Verify contract consistency
- [x] Confirm field names and types are identical across server, shared, and view-model layers.
- [x] Confirm the contracts support user-scoped filtering and append-only action history.
- [x] Confirm no changes are introduced to global `Book` or `Author` models unless necessary.

### 8) Validate completion
- [x] Build the touched projects or the solution.
- [x] Resolve any compile-time mismatches caused by the new contracts.
- [x] Mark WI-001 and WI-002 complete only after the contracts and validation rules are in place.

## Epic 1 Done When
- [x] WI-001 and WI-002 meet their acceptance criteria.
- [x] The solution builds successfully with the new contracts.
- [x] The contract surface is ready for the API endpoint work in Epic 2.

## Completion Note
Epic 1 contracts and validation scaffolding have been implemented in code.
