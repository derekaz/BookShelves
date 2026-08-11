# Versioning and Release Notes

This document records the repository's build metadata and release-flow reminders.
It is intended as a working reference for developers and automation.

## Versioning Sources

The solution uses GitVersion-based metadata through:

- `GitVersion.yml`
- `src/Directory.Build.Props`
- `src/global.json` for the SDK pin used by local and CI builds

## Important Build Outputs

Keep these values aligned when changing release behavior:

- `Version`
- `VersionPrefix`
- `AssemblyVersion`
- `FileVersion`
- `InformationalVersion`
- MAUI `ApplicationDisplayVersion`
- MAUI `ApplicationVersion`

## Platform Build Number Strategy

For CI publish jobs, MAUI app version properties are set by workflow outputs:

- `ApplicationDisplayVersion` -> GitVersion `major.minor.patch`
- Apple `ApplicationVersion` -> `github.run_number`
- Windows `ApplicationVersion` -> per-`major.minor.patch` incrementing counter

### Windows Counter Implementation

- Counter storage branch: `build-counters`
- Counter file format: `<major.minor.patch>.txt`
- Example: `10.1.2.txt` stores the latest Windows build number for `10.1.2`
- Workflow retries push conflicts to handle concurrent runs
- Workflow fails if computed Windows build number exceeds `65535`

## Release and CI Notes

- Validation runs on push, pull request (`main`/`release/**`), and manual dispatch.
- Publish jobs currently run for `main`, `release/*`, and valid version tags.
- Tags should follow `v<MAJOR>.<MINOR>.<PATCH>` with optional prerelease/build metadata.
- Verify effective version outputs locally and in CI for release-sensitive changes.

## Related Files

- `GitVersion.yml`
- `src/Directory.Build.Props`
- `.github/workflows/BookShelves Multi-Platform CI-CD.yml`
- `.github/RELEASE_PROCESS.md`
- `docs/Build-Test-Run.md`
