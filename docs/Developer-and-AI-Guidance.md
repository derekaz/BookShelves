# Developer and AI Contribution Guidance

This file provides practical guardrails for human and AI-assisted contributions.

## General Principles

- Keep changes minimal and focused on the requested scope.
- Favor existing patterns, dependencies, and solution structure over introducing new ones.
- Validate the affected area with build or tests before finalizing.
- Keep solution-level guidance in `docs/` and project-specific notes near the project.

## Where to Implement Changes

- UI and shared Blazor components: `src/BookShelves.Shared`
- Web host concerns and authentication pipeline: `src/BookShelves.Web/BookShelves.Web`
- Browser-only Blazor behavior: `src/BookShelves.Web/BookShelves.Web.Client`
- API endpoints and contracts: `src/BookShelves.WebApi`
- MAUI platform host behavior: `src/BookShelves.Maui`
- MAUI local data and sync: `src/BookShelves.Maui.Data`
- MAUI EF Core migration startup host: `src/BookShelves.Maui.MigrationHost`
- Web-oriented shared code: `src/BookShelves.Web.Shared`
- API test coverage: `test/BookShelves.WebApi.Tests`
- Shared library test coverage: `test/BookShelves.Shared.Tests`
- Web shared library test coverage: `test/BookShelves.Web.Shared.Tests`
- MAUI data test coverage: `test/BookShelves.Maui.Data.Tests`

## Safe Change Workflow

1. Identify the owning host project and impacted shared library.
2. Implement changes in the narrowest layer that solves the issue.
3. Add or update regression tests when fixing a bug.
4. Build impacted projects and run targeted tests.
5. Update docs when behavior, workflow, or architecture changes.

## AI-Assisted Work Recommendations

- Prefer symbol-aware navigation over broad text changes when possible.
- Avoid speculative refactors not required by the request.
- Do not modify generated or CI-owned artifacts unless explicitly required.
- Keep instructions and assumptions explicit in PR descriptions.
- Reuse existing test helpers/utilities before adding new ad-hoc setup in tests.
- Treat `docs/Notes.md` as historical reference material, not as the source of truth.

## Versioning and Branching Awareness

- Versioning is centrally controlled via `GitVersion.yml` and `src/Directory.Build.Props`.
- Use branch conventions consistently for main, release, feature, and pull-request flows.
- For release-sensitive changes, verify effective version outputs locally and in CI.
- When CI versioning logic changes, update both `docs/Versioning-and-Release.md` and `.github/RELEASE_PROCESS.md`.

## Documentation Hygiene

- Keep solution-level docs discoverable in `docs/`.
- Prefer short, task-focused markdown files over one large catch-all file.
- Cross-link related docs as this folder grows.
- Use `docs/README.md` as the entry point for solution-level documentation.
- Update `Solution-Structure.md` and `Build-Test-Run.md` when the solution layout or build workflow changes.
- Update `Testing-Strategy.md` when test scope, risk priorities, or quality gates change.
