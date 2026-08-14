# BookShelves Solution Structure

This document gives a quick orientation to the solution and project responsibilities.

## Repository Layout

- `src/` - application, host, and shared runtime projects
- `test/` - automated test projects
- `docs/` - solution-level guidance and operational notes
- `BookShelves (Maui, Web and WebApi).slnx` - the primary solution file
- `GitVersion.yml` - branch/version strategy
- `.editorconfig` - formatting and style rules

## Solution Projects and Intent

### Host and UI Projects

| Project | Intent | Notes |
| --- | --- | --- |
| `src/BookShelves.AppHost/BookShelves.AppHost.csproj` | .NET Aspire AppHost | Orchestrates the local web host and Web API development experience, including service discovery-friendly references and shared environment wiring. |
| `src/BookShelves.Maui/BookShelves.Maui.csproj` | .NET MAUI hybrid app host | Hosts the mobile and desktop experience for Android, iOS, MacCatalyst, and Windows. |
| `src/BookShelves.Web/BookShelves.Web/BookShelves.Web.csproj` | ASP.NET Core web host | Hosts the web app shell, auth pipeline, server-side concerns, and static web assets. |
| `src/BookShelves.Web/BookShelves.Web.Client/BookShelves.Web.Client.csproj` | Blazor WebAssembly client | Runs browser-side code for the web experience. |
| `src/BookShelves.WebApi/BookShelves.WebApi.csproj` | ASP.NET Core Web API | Serves data and endpoints used by the web and MAUI clients. |

### Shared Libraries

| Project | Intent | Notes |
| --- | --- | --- |
| `src/BookShelves.ServiceDefaults/BookShelves.ServiceDefaults.csproj` | Shared Aspire service defaults | Centralizes service discovery, resilience, health check, and OpenTelemetry defaults for the server-side hosts. |
| `src/BookShelves.Shared/BookShelves.Shared.csproj` | Shared Blazor UI, services, and models | Shared across MAUI, web, and API-adjacent code paths; multi-targeted for `net9.0` and `net10.0`. |
| `src/BookShelves.Web.Shared/BookShelves.Web.Shared.csproj` | Web-oriented shared code | Shared code used by the web host and web client. |

### Data and Migrations

| Project | Intent | Notes |
| --- | --- | --- |
| `src/BookShelves.Maui.Data/BookShelves.Maui.Data.csproj` | MAUI data access and sync support | Holds MAUI-side persistence and data synchronization behavior. |
| `src/BookShelves.Maui.MigrationHost/BookShelves.Maui.MigrationHost.csproj` | EF Core migration startup host | Console host used to generate and apply migrations for MAUI data. |

### Tests

| Project | Intent | Notes |
| --- | --- | --- |
| `test/BookShelves.WebApi.Tests/BookShelves.WebApi.Tests.csproj` | API test coverage | xUnit tests for Web API behavior and contracts. |
| `test/BookShelves.Shared.Tests/BookShelves.Shared.Tests.csproj` | Shared library test coverage | xUnit tests for shared models and services in `BookShelves.Shared`. |
| `test/BookShelves.Web.Shared.Tests/BookShelves.Web.Shared.Tests.csproj` | Web shared library test coverage | xUnit tests for web-shared DTO and view-model mapping behavior. |
| `test/BookShelves.Maui.Data.Tests/BookShelves.Maui.Data.Tests.csproj` | MAUI data test coverage | xUnit tests for MAUI data mapping and local unit-of-work persistence behavior. |
| `test/BookShelves.Web.Tests/BookShelves.Web.Tests.csproj` | Web host smoke/integration coverage | xUnit tests for core BookShelves.Web host endpoint and routing smoke behavior. |
| `test/BookShelves.Web.Client.Tests/BookShelves.Web.Client.Tests.csproj` | WebAssembly client test coverage | xUnit tests for BookShelves.Web.Client service and client-handler behavior. |

## Target Framework Summary

- Primary app/API hosts target `.NET 10`.
- Shared libraries include multi-targeting to `.NET 9` and `.NET 10` where required.
- MAUI targets platform-specific TFMs under `.NET 10` (Android, iOS, MacCatalyst, Windows).

## Where to Start for Common Changes

- UI and shared Blazor components: `src/BookShelves.Shared`
- MAUI platform startup and native concerns: `src/BookShelves.Maui`
- Local server orchestration and Aspire dashboard: `src/BookShelves.AppHost`
- Web host and authentication pipeline: `src/BookShelves.Web/BookShelves.Web`
- Browser-only Blazor behavior: `src/BookShelves.Web/BookShelves.Web.Client`
- API endpoints and contracts: `src/BookShelves.WebApi`
- MAUI local data and sync behavior: `src/BookShelves.Maui.Data`
- EF Core migration changes: `src/BookShelves.Maui.MigrationHost`
- API test coverage: `test/BookShelves.WebApi.Tests`
- Shared library test coverage: `test/BookShelves.Shared.Tests`
- Web shared library test coverage: `test/BookShelves.Web.Shared.Tests`
- MAUI data test coverage: `test/BookShelves.Maui.Data.Tests`
- Web host smoke/integration test coverage: `test/BookShelves.Web.Tests`
- WebAssembly client test coverage: `test/BookShelves.Web.Client.Tests`

## Practical Navigation Tips

- Start at the host project for runtime behavior.
- Move into shared libraries before duplicating logic across hosts.
- Keep platform-specific code in the relevant host or platform folder.
- Treat the solution structure as the default map for new work.
