# Build, Test, and Run Guide

This guide covers the common local development commands and workflows for the solution.

## Prerequisites

- .NET SDK pinned by `src/global.json` and the repo-level build props
- Platform prerequisites for MAUI workloads as needed for Android, iOS, MacCatalyst, and Windows
- Optional: Docker Desktop for containerized web + API runs

## Restore

From the repo root:

```powershell
dotnet restore "BookShelves (Maui, Web and WebApi).slnx"
```

## Build

From the repo root:

```powershell
dotnet build "BookShelves (Maui, Web and WebApi).slnx"
```

When working in a single project, prefer building that project first and then validate the solution before finishing.

Build behavior includes shared version metadata from `src/Directory.Build.Props` and `GitVersion.yml`.

## Test

Run all tests:

```powershell
dotnet test "BookShelves (Maui, Web and WebApi).slnx"
```

Run only Web API tests:

```powershell
dotnet test test/BookShelves.WebApi.Tests/BookShelves.WebApi.Tests.csproj
```

Run only shared library tests:

```powershell
dotnet test test/BookShelves.Shared.Tests/BookShelves.Shared.Tests.csproj
```

Run only web shared tests:

```powershell
dotnet test test/BookShelves.Web.Shared.Tests/BookShelves.Web.Shared.Tests.csproj
```

Run only MAUI data tests:

```powershell
dotnet test test/BookShelves.Maui.Data.Tests/BookShelves.Maui.Data.Tests.csproj
```

## Run Web + WebApi with Aspire

From the repo root:

```powershell
dotnet run --project src/BookShelves.AppHost/BookShelves.AppHost.csproj
```

Use Aspire for the day-to-day local development experience for `BookShelves.Web` and `BookShelves.WebApi`, including orchestration, logs, traces, and health endpoints.
The MAUI app remains outside AppHost orchestration and continues using its own backend configuration.

## Run Web + WebApi with Docker Compose

From the `src/` directory:

```powershell
docker compose --env-file .env.development-laptop up --build -d
```

Expected exposed ports:
- Web app: `http://localhost:5000`
- Web API: `http://localhost:5001`

Use Docker Compose when validating the containerized nginx front-door flow.
For routing and proxy details, see `docs/Docker-and-Networking.md`.

Stop containers:

```powershell
docker compose down
```

## Validate Web API Auth from PowerShell

Use `src/BookShelves.WebApi/Invoke-BookShelvesWebApiCheck.ps1` for a quick CLI check against either the direct Web API container or the deployed nginx front door.

Quick reachability check against a protected endpoint:

```powershell
./src/BookShelves.WebApi/Invoke-BookShelvesWebApiCheck.ps1 -BaseUrl "https://bookshelves.azmoore.com" -SkipToken
```

Authenticated check with client credentials:

```powershell
./src/BookShelves.WebApi/Invoke-BookShelvesWebApiCheck.ps1 -BaseUrl "https://bookshelves.azmoore.com" -TenantId "<tenant-id>" -ClientId "<client-id>" -ClientSecret "<client-secret>"
```

For local Docker Compose validation, point the script at `http://localhost:5001` and use `-Path "/Test"`.

## EF Core Migration Workflow (MAUI Data)

For MAUI data model changes, use the migration host project.

Example from the repo root:

```powershell
dotnet ef migrations add <MigrationName> --project ./src/BookShelves.Maui.Data --startup-project ./src/BookShelves.Maui.MigrationHost
```

## Formatting

Before commits, apply repository formatting guidance:

```powershell
dotnet format "BookShelves (Maui, Web and WebApi).slnx" --no-restore --exclude Templates/src --exclude-diagnostics CA1822
```

## CI and Container Packaging Notes

- GitHub Actions still validates and publishes the deployable server containers from `src/BookShelves.Web/BookShelves.Web/Dockerfile` and `src/BookShelves.WebApi/Dockerfile`.
- The validation job now runs the four solution test suites (`WebApi`, `Shared`, `Web.Shared`, and `Maui.Data`) and uploads TRX + coverage artifacts for each run.
- See `docs/CI-Test-Artifacts.md` for artifact triage and interpretation guidance.
- The new AppHost is for local orchestration and is not packaged or deployed as a production container in the current workflow.
- The new `BookShelves.ServiceDefaults` project is a shared library only; it affects container restore/build inputs but does not produce its own image.
- Container deployment still uses the existing separate web and Web API image model.
- A scheduled flaky-monitor workflow (`.github/workflows/BookShelves-Flaky-Tests-Monitor.yml`) reruns test projects multiple times and publishes TRX artifacts for reliability triage.
- MAUI publish jobs in CI override `ApplicationDisplayVersion` and `ApplicationVersion` at publish time.
- Windows publish uses a per-`major.minor.patch` incrementing `ApplicationVersion` for MSIX packaging.

## Versioning Notes

- The SDK pin lives in `src/global.json`.
- Shared version properties are centralized in `src/Directory.Build.Props`.
- Local fallback version values keep developer builds working outside CI.
- Local MAUI builds may use fallback `ApplicationVersion` values when CI outputs are not supplied.
- See `docs/Versioning-and-Release.md` for branch, build, and release details.
