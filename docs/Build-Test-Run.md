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

## Run Web + WebApi with Docker Compose

From the `src/` directory:

```powershell
docker compose --env-file .env.development-laptop up --build -d
```

Expected exposed ports:
- Web app: `http://localhost:5000`
- Web API: `http://localhost:5001`

For routing and proxy details, see `docs/Docker-and-Networking.md`.

Stop containers:

```powershell
docker compose down
```

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

## Versioning Notes

- The SDK pin lives in `src/global.json`.
- Shared version properties are centralized in `src/Directory.Build.Props`.
- Local fallback version values keep developer builds working outside CI.
- See `docs/Versioning-and-Release.md` for branch, build, and release details.
