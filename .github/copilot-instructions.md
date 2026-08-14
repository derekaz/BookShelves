# GitHub Copilot Development Environment Instructions

This document provides repository-specific guidance for BookShelves. Use it together with the solution docs in `docs/`.

## Repository Overview

BookShelves is a cross-platform book library application with shared Blazor UI, a .NET MAUI host, a web host, a Blazor WebAssembly client, and a Web API. The solution targets .NET 10 and .NET 9 where required.

## Source of Truth

- `docs/README.md` - documentation index
- `docs/Solution-Structure.md` - project map and ownership guidance
- `docs/Build-Test-Run.md` - restore, build, test, run, format, and migration commands
- `docs/Docker-and-Networking.md` - Docker and nginx routing notes
- `docs/Versioning-and-Release.md` - versioning and release-flow notes
- `docs/Testing-Strategy.md` - solution-wide testing baseline and quality gates
- `docs/Developer-and-AI-Guidance.md` - solution-level contributor guidance

## Current Project Structure

- `src/BookShelves.Maui/` - .NET MAUI app host for Android, iOS, MacCatalyst, and Windows
- `src/BookShelves.Maui.Data/` - MAUI data access and sync support
- `src/BookShelves.Maui.MigrationHost/` - EF Core migration startup host for MAUI data
- `src/BookShelves.Shared/` - shared Blazor UI, services, and models
- `src/BookShelves.Web/BookShelves.Web/` - ASP.NET Core web host
- `src/BookShelves.Web/BookShelves.Web.Client/` - Blazor WebAssembly client
- `src/BookShelves.Web.Shared/` - web-oriented shared code
- `src/BookShelves.WebApi/` - ASP.NET Core Web API
- `test/BookShelves.WebApi.Tests/` - API test coverage
- `test/BookShelves.Shared.Tests/` - shared library test coverage
- `test/BookShelves.Web.Shared.Tests/` - web shared library test coverage
- `test/BookShelves.Maui.Data.Tests/` - MAUI data test coverage

## Development Workflow

- Keep changes minimal and focused on the requested scope.
- Prefer existing patterns, shared libraries, and current solution structure.
- Build and test the affected area before finalizing changes.
- Update the relevant docs in `docs/` when behavior, workflow, or architecture changes.
- Defer branch protection required-check changes until test coverage reaches a reasonable level after completing test improvements.
- For current testing strategy work, skip creating a separate dedicated PR validation workflow for now.
- Pause additional Web.Client-focused test expansion for now and shift effort elsewhere.

### Restore and Build

```powershell
dotnet restore "BookShelves (Maui, Web and WebApi).slnx"
dotnet build "BookShelves (Maui, Web and WebApi).slnx"
```

### Test

```powershell
dotnet test "BookShelves (Maui, Web and WebApi).slnx"
dotnet test test/BookShelves.WebApi.Tests/BookShelves.WebApi.Tests.csproj
dotnet test test/BookShelves.Shared.Tests/BookShelves.Shared.Tests.csproj
dotnet test test/BookShelves.Web.Shared.Tests/BookShelves.Web.Shared.Tests.csproj
dotnet test test/BookShelves.Maui.Data.Tests/BookShelves.Maui.Data.Tests.csproj
```

- When reporting test coverage numbers, always exclude migration/generated files from the metrics.

### Format

```powershell
dotnet format "BookShelves (Maui, Web and WebApi).slnx" --no-restore --exclude Templates/src --exclude-diagnostics CA1822
```

## Platform-Specific Development

- Android: requires Android SDK and OpenJDK 17
- iOS: requires Xcode and, when developing on Windows, pairing to a Mac
- MacCatalyst: requires Xcode
- Windows: requires the Windows SDK

## Logging Guidance

For the MAUI app, logging should avoid `SpecialFolder.Desktop` and use `Documents/AZMoore/BookShelves/logs`, while keeping the database path unchanged.

## Contribution Guidelines

- Prefer symbol-aware navigation over broad text changes when possible.
- Avoid speculative refactors not required by the request.
- Do not modify generated or CI-owned artifacts unless explicitly required.
- Keep instructions and assumptions explicit in PR descriptions.
- Add a regression test for each production bug fix when practical.
- Reuse test helpers/utilities for repeated setup patterns before introducing new fixtures.
- Follow `docs/Testing-Strategy.md` ownership mapping when deciding which test project should cover a change.
- Use `docs/README.md` as the entry point for solution-level documentation.
- Prefer splitting broad notes into topic-specific docs when a subject becomes stable.

## Web and API Docker Setup

The web/API Docker setup uses nginx as a front door for `/` and `/api` only. It does not do application-level proxying beyond those paths.