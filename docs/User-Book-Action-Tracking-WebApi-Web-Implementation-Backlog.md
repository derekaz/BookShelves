# User Book Action Tracking (Web/API First) — Implementation Backlog

## Guardrails

- Reuse existing online/offline Datasync flow and conventions (table endpoints, DTO shape, sync metadata, auth pipeline).
- `Books` and `Authors` remain global entities.
- User-book tracking data is user-scoped:
  - Non-admin users can only read/write their own related records.
  - Admin users can access cross-user data by policy.
- Cosmos DB should use the existing shared container.

---

## Epic 1 — Domain + Contracts (P0)

### 1. Create user-book action entity contracts
- Add `BookUserAction` in:
  - `src/BookShelves.WebApi/...` (server table model)
  - `src/BookShelves.Web.Shared/Data/...` (shared DTO)
  - `src/BookShelves.Shared/Presentation/ViewModels/...` (view model)
- Include: `BookId`, `UserId`, `ActionType`, `OccurredAtUtc`, optional `PagesRead`, `Notes`.

### 2. Define action types and validation rules
- Add enum/constants for action types (`ToBeRead`, `PagesRead`, `Finished`, etc.).
- Add validation rules (non-negative pages, required `BookId`, etc.).

---

## Epic 2 — WebApi Datasync Table + Security (P0)

### 3. Add Datasync table endpoint
- Add `BookUserActionsController : TableController<BookUserAction>` in `src/BookShelves.WebApi/Controllers`.
- Route should follow existing pattern: `/tables/BookUserActions`.

### 4. Add access control provider for user scoping
- Add `BookUserActionsAccessControlProvider` in `src/BookShelves.WebApi/...DataAccess`.
- `GetDataView()`:
  - Non-admin => `entity.UserId == currentUserId`
  - Admin => all records
- `IsAuthorizedAsync(...)`:
  - Enforce same rule for create/update/delete.
  - Ignore client-supplied `UserId` for non-admin writes; set from auth token.

### 5. Register Cosmos repository + table options
- Update `src/BookShelves.WebApi/Program.cs`:
  - Register `ICosmosTableOptions<BookUserAction>` using current DB/container pattern.
  - Register `IRepository<BookUserAction>` via existing `CachedCosmosRepository<T>` pattern.

---

## Epic 3 — Web Server Service Layer (P0)

### 6. Add server-side Datasync client factory + service
- Add:
  - `BookUserActionsDatasyncClientFactory` (`src/BookShelves.Web/BookShelves.Web/Services`)
  - `BookUserActionsDataService` (`src/BookShelves.Web/BookShelves.Web/Services/Server`)
- Follow existing `BooksDataService`/`AuthorsDataService` conventions.

### 7. Add web server endpoints for UI usage
- In `src/BookShelves.Web/BookShelves.Web/Program.cs`, add endpoints similar to `/booksdata`:
  - `/bookuseractionsdata` GET/POST/PUT/DELETE (or POST + GET for MVP)
- Require authorization on all endpoints.

---

## Epic 4 — Blazor UI (P1)

### 8. Add book-level action UI
- Start in:
  - `src/BookShelves.Shared/Components/Pages/Books/BookDetail.razor`
  - optionally `src/BookShelves.Shared/Components/Pages/Books/Index.razor`
- Add actions:
  - Add to To-Be-Read
  - Log pages read
  - Mark finished

### 9. Add action history/progress display
- Add a timeline/progress panel on Book Detail.
- Read from `/bookuseractionsdata` (implicitly scoped to current user unless admin).

---

## Epic 5 — Authorization Policy Refinement (P0)

### 10. Formalize admin policy usage
- Reuse `AuthorizationPolicies.AdminAccess` where needed.
- Ensure explicit behavior coverage for:
  - own-record access allowed
  - cross-user access denied for non-admin
  - cross-user access allowed for admin

---

## Epic 6 — Tests (P0)

### 11. WebApi tests
- Add tests in `test/BookShelves.WebApi.Tests` for:
  - anonymous rejected
  - authenticated user sees only own actions
  - non-admin cannot modify another user record
  - admin can read/modify across users
- Mirror current test patterns used for books/authors/auth restriction.

### 12. Web server/service tests
- Add tests in `test/BookShelves.Web.Tests` for:
  - new Datasync factory base-url behavior
  - service error handling/token-acquisition behavior
  - endpoint auth behavior for `/bookuseractionsdata`

---

## Suggested Delivery Slices

### Slice A (MVP, P0)
- Epics 1, 2, 3, 5, and 6 (no UI yet).

### Slice B (P1)
- Epic 4 UI integration.

### Slice C (Later)
- MAUI offline table + sync parity using the same entity and access rules.
