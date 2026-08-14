# Testing Strategy

This document defines the solution-wide testing baseline and phased strategy for growing test coverage.

## Baseline Snapshot (Phase 1 + Phase 2)

Current automated test projects in the solution:

| Project | Primary scope | Current test count* |
| --- | --- | --- |
| `test/BookShelves.Shared.Tests` | Shared models and shared services (`BookShelves.Shared`) | 48+ |
| `test/BookShelves.WebApi.Tests` | API auth, controller, and repository caching behavior (`BookShelves.WebApi`) | 15+ |
| `test/BookShelves.Web.Shared.Tests` | Web shared DTO and mapping behavior (`BookShelves.Web.Shared`) | 5 |
| `test/BookShelves.Maui.Data.Tests` | MAUI data mapping and local unit-of-work behavior (`BookShelves.Maui.Data`) | 5 |

\* Counts are based on local test discovery and are expected to change as coverage grows.

## Test Taxonomy

Use these categories consistently across the solution:

- **Unit tests**: Pure logic tests with no external process or network dependency.
- **Integration tests**: Host plus dependency interaction tests (HTTP pipeline, persistence, auth integration, or sync boundaries).
- **Contract tests**: API behavior assertions for status codes, payload shape, and authorization requirements.
- **UI/component smoke tests**: Minimal coverage of critical user journeys and rendering behavior.

## Coverage Gap Inventory (Post-Phase 2)

| Area | Current state | Priority | Recommended next test type |
| --- | --- | --- | --- |
| `src/BookShelves.Shared` | Unit coverage exists and is expanding, but major component/service paths remain | High | Unit + component |
| `src/BookShelves.WebApi` | Auth/controller tests plus caching tests exist; endpoint validation and persistence contracts still incomplete | High | Integration + contract |
| `src/BookShelves.Web.Shared` | Dedicated tests now cover core mapping behavior; broader DTO edge-case coverage still limited | Medium | Unit |
| `src/BookShelves.Maui.Data` | Dedicated tests now cover mapping and local unit-of-work behavior; sync pipeline integration remains limited | High | Integration |
| `src/BookShelves.Web/BookShelves.Web` | No dedicated test project in solution | Medium | Integration |
| `src/BookShelves.Web/BookShelves.Web.Client` | No dedicated test project in solution | Medium | UI/component smoke |
| `src/BookShelves.Maui` | No dedicated test project in solution | Medium | Unit (extracted shared logic) + targeted integration |
| `src/BookShelves.ServiceDefaults` | No dedicated test project in solution | Low | Unit |

## Risk Map for Test Expansion

Prioritize test additions by risk to production behavior:

1. **Authentication and authorization paths** (`BookShelves.WebApi`, web host integration)
2. **Data sync and local persistence correctness** (`BookShelves.Maui.Data`, shared sync orchestration)
3. **API contract stability** (status codes, validation responses, payload shape)
4. **Critical shared business logic** (`BookShelves.Shared` services/models used by multiple hosts)
5. **Core user-path smoke coverage** (web client and shared UI components)

## Quality Gates (Phase 3)

Adopt these as the current default quality bar:

1. All test projects in the solution must pass before merge.
2. Pull request validation must execute the core test projects in CI (`WebApi`, `Shared`, `Web.Shared`, and `Maui.Data`).
3. CI should publish test evidence for every validation run:
   - TRX test result artifacts
   - Cobertura coverage artifacts
4. Every production bug fix should add or update a regression test in the relevant test project.
5. New test projects should be added under `test/` and reflected in `docs/Solution-Structure.md` and `docs/Build-Test-Run.md`.

## Flaky Test Monitoring

- Use `.github/workflows/BookShelves-Flaky-Tests-Monitor.yml` for scheduled reliability checks.
- The flaky monitor reruns each core test project three times on Ubuntu and publishes TRX outputs for triage.
- If a repeated run fails, treat it as a reliability issue and either fix the test or isolate the dependency causing instability.

## Regression and Maintainability Strategy (Phase 4)

- Every production bug fix should include a regression test that would fail before the fix.
- Prefer shared test helpers/builders for repeated setup (for example, authenticated client setup and common model creation).
- Keep tests deterministic by avoiding time/network randomness unless explicitly controlled by the test.
- When a test is flaky or brittle, either stabilize it in the same PR or quarantine with a follow-up issue before merge.

## Test Conventions

- Use clear `MethodName_Condition_ExpectedResult` naming for new tests.
- Follow Arrange-Act-Assert structure.
- Keep one behavioral assertion focus per test.
- Use project-level test utilities before introducing new mocking or fixture patterns.

## Project Test Ownership Map (Phase 5)

| Runtime project area | Primary test project owner | Ownership expectation |
| --- | --- | --- |
| `src/BookShelves.WebApi` | `test/BookShelves.WebApi.Tests` | Maintain API auth/contract/integration coverage and add regression tests for endpoint defects. |
| `src/BookShelves.Shared` | `test/BookShelves.Shared.Tests` | Maintain shared service/model logic coverage and protect cross-host behavior changes. |
| `src/BookShelves.Web.Shared` | `test/BookShelves.Web.Shared.Tests` | Maintain DTO/view-model mapping and client-shared data contract coverage. |
| `src/BookShelves.Maui.Data` | `test/BookShelves.Maui.Data.Tests` | Maintain local persistence/sync-adjacent behavior coverage and mapping integrity. |
| `src/BookShelves.Web/BookShelves.Web` | `test/BookShelves.WebApi.Tests` (until dedicated suite exists) | Add boundary tests when server-host behavior changes impact auth/routing/API integration. |
| `src/BookShelves.Web/BookShelves.Web.Client` | `test/BookShelves.Web.Shared.Tests` (until dedicated suite exists) | Add smoke/behavior tests for browser-facing changes that affect shared contracts. |
| `src/BookShelves.Maui` | `test/BookShelves.Maui.Data.Tests` + `test/BookShelves.Shared.Tests` (until dedicated suite exists) | Cover extracted shared logic and add integration checks around MAUI-specific orchestration. |

## Ownership and Review Expectations

- Contributors add tests in the nearest project that owns the behavior.
- Reviewers should verify test scope matches the changed layer (unit vs integration vs contract).
- If behavior crosses layers, include at least one boundary test at the integration/contract level.
- Reviewers should confirm bug-fix PRs include a regression test when applicable.
- Ownership for uncovered areas should be made explicit in the PR description when no dedicated suite exists.
