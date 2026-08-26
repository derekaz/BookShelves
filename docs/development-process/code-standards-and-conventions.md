# Code Standards and Conventions

This document formalizes the coding standards, style conventions, and architectural patterns used throughout the BookShelves solution.

---

## C# Coding Conventions

All code follows the conventions defined in `.editorconfig` at the repository root. Key standards:

### Formatting

- **Indentation**: 4 spaces (no tabs)
- **Line endings**: CRLF
- **File ending**: No final newline
- **Tab width**: 4

### Naming Conventions

- **Classes, interfaces, records**: PascalCase
- **Methods, properties**: PascalCase
- **Local variables, parameters**: camelCase
- **Private fields**: camelCase (no leading underscore)
- **Constants**: PascalCase
- **Namespace**: Match folder structure (e.g., `BookShelves.Shared.Services` for `src/BookShelves.Shared/Services`)

Use `dotnet_style_namespace_match_folder = true` — folders and namespaces must align.

### Access Modifiers

- Explicitly declare access modifiers for all non-interface members (`dotnet_style_require_accessibility_modifiers = for_non_interface_members`)
- Prefer `public`, `private`, `protected` over implicit defaults
- Use `internal` for cross-project shared code within the solution

### Type Preferences

- Prefer predefined types (e.g., `string`, `int`, `bool`) over BCL types (`System.String`, `System.Int32`)
- Apply this consistently for both locals/parameters and member access

### Expression and Operator Style

- **Parentheses**: Use `always_for_clarity` for arithmetic and relational operators
- **Null propagation**: Prefer `?.` operator over null checks
- **Coalescing**: Prefer `??` and `??=` operators
- **Object initializers**: Prefer object initializers over repeated assignments
- **Collection initializers**: Use collection expression syntax when types loosely match
- **Conditional operators**: Prefer ternary over if/else for simple assignments

### Variable Declarations

- Use explicit types (not `var`) for built-in types and non-obvious types
- Use `var` only when the type is immediately obvious from context
- Apply this consistently: `csharp_style_var_* = false` (except implicitly obvious cases)

### Auto-Properties and Fields

- Prefer auto-properties (`{ get; set; }`) over backing fields
- Mark backing fields `readonly` when possible
- Unused parameters should be explicitly flagged (code quality: `all`)

### Expression-Bodied Members

- **Accessors**: Prefer expression bodies
- **Indexers**: Prefer expression bodies
- **Lambdas**: Prefer expression bodies
- **Constructors**: Use traditional block syntax (no expression bodies)

---

## Project Structure and Layering

### Source Organization

```
src/
  BookShelves.Shared/              → Shared Blazor UI, models, and cross-host services
  BookShelves.Maui/                → .NET MAUI app host
  BookShelves.Maui.Data/           → MAUI data access and sync
  BookShelves.Maui.MigrationHost/  → EF Core migration host
  BookShelves.Web/                 → ASP.NET Core web host
	BookShelves.Web.Client/        → Blazor WebAssembly client
  BookShelves.Web.Shared/          → Web-specific shared code
  BookShelves.WebApi/              → ASP.NET Core Web API
  BookShelves.ServiceDefaults/     → Aspire service defaults
  BookShelves.AppHost/             → Aspire AppHost orchestration

test/
  BookShelves.WebApi.Tests/
  BookShelves.Shared.Tests/
  BookShelves.Web.Tests/
  BookShelves.Web.Client.Tests/
  BookShelves.Web.Shared.Tests/
  BookShelves.Maui.Data.Tests/
```

### Layering Rules

1. **API Layer** (`src/BookShelves.WebApi`)
   - Controllers, endpoints, HTTP contracts
   - Authorization and authentication logic
   - Depends on: `Shared`, `ServiceDefaults`

2. **Service Layer** (`src/BookShelves.Web`, `src/BookShelves.Maui`)
   - Business logic orchestration
   - Client factories and data services
   - Depends on: `Shared`, `Web.Shared` (or platform-specific shared)

3. **Data Layer** (`src/BookShelves.Maui.Data`)
   - Persistence and sync abstractions
   - Entity mappings and repositories
   - Depends on: `Shared`

4. **Shared Layer** (`src/BookShelves.Shared`, `src/BookShelves.Web.Shared`)
   - Models, DTOs, view models
   - Cross-platform services
   - UI components (Blazor/Razor)
   - No dependencies on host projects

5. **Infrastructure** (`src/BookShelves.ServiceDefaults`)
   - Service discovery, resilience, observability
   - Shared configuration defaults
   - No business logic

### Dependency Direction

```
API / Web / MAUI
	↓
Shared Service Layer
	↓
Data Abstractions
	↓
Shared Models & Infrastructure
```

**Never reverse dependencies** — shared layers must not depend on host projects.

---

## Architectural Patterns

### Data Access

- **Entity Framework Core**: Used for MAUI local persistence and API-layer queries
- **Repository Pattern**: Encapsulate data access for testability
- **Unit of Work**: Implicit via EF Core DbContext in MAUI
- **DTOs and Mapping**: Map domain models to contracts at API/service boundaries

### Service Design

- **Dependency Injection**: Constructor injection via `IServiceCollection`
- **Factory Pattern**: Used for client creation and cross-host data service factories
- **Async/Await**: Always use async for I/O and network operations
- **Observability**: Integrate with OpenTelemetry via `ServiceDefaults`

### Authorization

- **Per-User Data Isolation**: Non-admin users can only access their own related data; admins can access broader data
- **Policy-Based Authorization**: Use authorization policies (e.g., `admin` policy) at endpoint/controller level
- **Access Control Provider**: Encapsulate user scope validation in dedicated providers

### Testing

- Separate test projects by layer (unit, integration, contract)
- Use `Arrange-Act-Assert` structure
- Avoid test interdependencies
- Mock external dependencies; test behavior at integration boundaries
- See `docs/Testing-Strategy.md` for detailed ownership and quality gates

---

## Multi-Targeting and Platform Concerns

### Target Frameworks

- Primary hosts: `.NET 10`
- Shared libraries: Multi-targeted to `.NET 9` and `.NET 10` where required
- MAUI: Platform-specific TFMs under `.NET 10` (Android, iOS, MacCatalyst, Windows)

### Platform-Specific Code

- Place platform-specific logic in the owning host project (e.g., `src/BookShelves.Maui` for MAUI concerns)
- Extract shared logic to `src/BookShelves.Shared` if used by multiple hosts
- Use conditional compilation sparingly; prefer abstraction and dependency injection

### Async Considerations

- Web API and Web host: Use async throughout
- MAUI: Use async for network; local persistence can be sync or async
- WebAssembly: Use async for server calls; prefer sync for local operations

---

## Documentation and Comments

### Inline Comments

- Avoid redundant comments that restate code
- Explain *why* non-obvious decisions were made
- Document complex algorithms, edge cases, and performance considerations
- Keep comments close to the code they describe

### XML Documentation

- Add XML doc comments (`///`) to public APIs
- Include `<summary>`, `<param>`, `<returns>`, and `<exception>` tags
- Example:

```csharp
/// <summary>
/// Retrieves a user's book with per-user isolation enforcement.
/// </summary>
/// <param name="userId">The owning user's ID.</param>
/// <param name="bookId">The book ID to retrieve.</param>
/// <returns>The book if authorized; null otherwise.</returns>
public async Task<UserBook?> GetUserBookAsync(string userId, int bookId)
{
	// implementation
}
```

### Solution-Level Documentation

- Keep high-level docs in `docs/` (architecture, process, decisions)
- Keep project-specific guidance in project folders (`src/[Project]/README.md`)
- Update `docs/README.md` when adding new guidance documents
- Use cross-links between related docs

---

## Code Review Standards

### What to Look For

- ✅ Follows naming, style, and formatting conventions
- ✅ No circular dependencies or layer violations
- ✅ Appropriate test coverage for the layer (unit/integration/contract)
- ✅ Async/await patterns are correct
- ✅ Error handling and logging are present where appropriate
- ✅ Authorization checks are in place for data access
- ✅ Configuration and secrets are managed securely
- ✅ No speculative refactors unrelated to the PR scope
- ✅ Commit messages are clear and atomic

### Common Issues to Flag

- ❌ Reversed dependencies (shared layer depending on host)
- ❌ `var` used for non-obvious types
- ❌ Synchronous blocking calls in async contexts
- ❌ Missing authorization checks
- ❌ Untested business logic
- ❌ Magic strings or numbers without explanation
- ❌ Broad exception catches without re-throw or logging

---

## Formatting and Linting

### Automatic Formatting

Run before committing:

```powershell
dotnet format "BookShelves (Maui, Web and WebApi).slnx" --no-restore --exclude Templates/src --exclude-diagnostics CA1822
```

### EditorConfig Validation

- All `.cs` files are validated against `.editorconfig`
- Configure your IDE to show EditorConfig violations
- Fix violations before committing (format command handles most automatically)

### Analyzer Rules

- CA1822: Mark members as static if they don't use instance data (excluded from format)
- Use StyleCop rules for consistency where applicable
- Suppress rules only with documented justification

---

## Summary Checklist

Before committing code:

- [ ] Naming follows conventions (PascalCase for types, camelCase for variables)
- [ ] Namespaces match folder structure
- [ ] Access modifiers are explicit
- [ ] No circular dependencies or layer violations
- [ ] Async/await is used correctly
- [ ] Error handling and logging are present
- [ ] Authorization checks are in place where needed
- [ ] Code formatted with `dotnet format`
- [ ] Tests added/updated with changes
- [ ] Commit message is clear and atomic
