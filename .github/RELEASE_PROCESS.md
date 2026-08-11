Release branch and CI/CD process
================================

Branch naming and intent
- main: protected trunk branch.
- release/<MAJOR>.<MINOR>.<PATCH>: release branch for a specific semantic series (example: release/10.1.2).
- feature/<name>: feature work branches.

Workflow triggers
- Validation runs on push to branches, pull requests to `main`/`release/**`, and manual `workflow_dispatch`.
- Publish jobs (containers, Apple, Windows) run for:
  - `main`
  - `release/*`
  - valid tags that match `v<MAJOR>.<MINOR>.<PATCH>` with optional prerelease/build metadata

Per-platform build numbering
- The validate job exposes:
  - `DisplayVersion` = semantic version (`major.minor.patch`)
  - `AppleBuildNumber` = `github.run_number`
  - `WindowsBuildNumber` = per-version incrementing counter
  - `FullSemVer` and `InformationalVersion`
- Apple builds use:
  - `/p:ApplicationDisplayVersion="${{needs.validate.outputs.DisplayVersion}}"`
  - `/p:ApplicationVersion=${{needs.validate.outputs.AppleBuildNumber}}`
- Windows builds use:
  - `/p:ApplicationDisplayVersion="${{needs.validate.outputs.DisplayVersion}}"`
  - `/p:ApplicationVersion=${{needs.validate.outputs.WindowsBuildNumber}}`

Windows per-version counter details
- Counter state is stored on the `build-counters` branch.
- One file per semantic line: `<major.minor.patch>.txt`.
- Each successful publish increments the value by 1 for that semantic line.
- The workflow retries push conflicts to handle concurrent runs.
- A safeguard fails the run if the computed Windows build number exceeds `65535`.

Tag and environment mapping
- Environment assignment is computed automatically:
  - Exact release tag `vX.Y.Z` -> `production`
  - Prerelease tag `vX.Y.Z-alpha*` -> `alpha`
  - Prerelease tag `vX.Y.Z-beta*` -> `beta`
  - Prerelease tag `vX.Y.Z-rc*` -> `rc`
  - `release/*` branch -> `beta`
  - `main`/`master` branch -> `alpha`
  - `feature/*` branch -> `development`
  - Any other ref -> `development`

Manual approvals and environment protection
- Publish jobs are assigned to the computed environment name.
- Configure environment protection rules in repository settings (Settings -> Environments -> <environment-name>) to require reviewers or wait timers.
- Environment protection configuration is done in GitHub UI by a repository admin.

Questions or changes
- If environment naming or publish gates need to change, update `.github/workflows/BookShelves Multi-Platform CI-CD.yml` and this file together.
