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

## Release and CI Notes

- Main branch commits should produce build artifacts
- Tags should drive release and deploy workflows
- Feature branches should flow through pull requests into release branches when needed
- Verify effective version outputs locally and in CI for release-sensitive changes

## Related Files

- `GitVersion.yml`
- `src/Directory.Build.Props`
- `docs/Build-Test-Run.md`
