# Release Process

This document outlines how features move from development to production in the BookShelves repository.

**Related document**: See `docs/Versioning-and-Release.md` for build metadata, version sources, and CI configuration details.

---

## Overview

BookShelves follows a **continuous merge and periodic release** model:

- Features are developed on feature branches and merged to `main` continuously
- `main` is always stable and deployable
- Releases are cut from `main` on a regular cadence (TBD: daily, weekly, etc.)
- Release notes are generated from commit history and pull requests

---

## Versioning Strategy

Versioning is centrally controlled via:

- `src/Directory.Build.Props` — Version number and build metadata
- `GitVersion.yml` — Branch-specific versioning rules

### Version Format

```
<Major>.<Minor>.<Patch>+<Metadata>
```

- **Major**: Feature or breaking changes (manual increment)
- **Minor**: New backward-compatible features
- **Patch**: Bug fixes and patches
- **Metadata**: Build ID, branch name, commit hash (automatic via CI)

### Branch Versioning Rules

See `GitVersion.yml` for authoritative branch rules. Key points:

- **main**: Pre-release or stable version (determined by release configuration)
- **feature/\***: Feature-branch versions with commit count
- **release/\***: Release branch versions (if used for release stabilization)
- **hotfix/\***: Hotfix versions (if used for production emergency fixes)

### Version Output

Version information is available via:

- Build output: Check console logs for version metadata
- NuGet packages: Embedded in package metadata
- Runtime: Query version from `AssemblyVersion` or API version endpoint

---

## Continuous Integration and Build

All CI workflows are defined in `.github/workflows/`:

- **On every commit to `main`**: Build, test, and validate
- **On pull requests**: Build, test, format check, and code analysis
- **Nightly/scheduled**: Flaky test monitoring and extended test runs

### Build Status

- All tests must pass before merge
- No merge conflicts allowed
- Code formatting must validate
- No new analyzer warnings

### Artifacts Published by CI

- Test results (TRX) — XML test report for analysis
- Coverage reports (Cobertura) — Code coverage metrics
- Build logs — For debugging build failures

See `docs/CI-Test-Artifacts.md` for how to access and interpret these artifacts.

---

## Release Workflow

The release workflow uses dedicated `release/[major.minor.patch]` branches for stabilization.

### Release Planning

1. **Determine Release Scope**
   - Identify features and fixes to be included
   - Review feature branches ready for main
   - Plan release date and version number

2. **Merge Features to main**
   - All feature branches for this release merge to `main` via pull request
   - Each PR is reviewed and passes CI
   - `main` represents the latest development work

### Creating and Stabilizing a Release

1. **Create Release Branch from main**

   ```powershell
   git switch main
   git pull origin main
   git switch -c release/1.2.0
   git push -u origin release/1.2.0
   ```

   **What this means:**
   - `release/1.2.0` is now in **beta** state
   - `main` continues accepting new features for the *next* release
   - Only stabilization and critical fixes go on the release branch

2. **Stabilization Phase on Release Branch**
   - Fix critical bugs discovered during testing
   - No new features should be added
   - Run comprehensive testing and QA
   - Create feature branches off the release branch for fixes if needed:

   ```powershell
   git switch release/1.2.0
   git switch -c feature/hotfix-release-issue
   # ... fix ... 
   # PR targets release/1.2.0 (not main)
   ```

3. **Tag the Release (Final Step)**

   Once the release is stabilized and ready:

   ```powershell
   git switch release/1.2.0
   git pull origin release/1.2.0

   # Tag with clean semantic version
   git tag v1.2.0
   git push origin v1.2.0

   # Build final release artifacts (this is the last action on this branch)
   # NuGet packages, app builds, deployment artifacts, etc.
   ```

4. **Merge Release Back to main (via PR)**

   **After tagging**, merge the release branch back to main to capture version metadata.

   **Since `main` is a protected branch, this requires a PR:**

   ```powershell
   # Prepare the merge locally
   git switch main
   git pull origin main
   git merge release/1.2.0
   # Don't push directly - will be blocked by branch protection
   ```

   **Instead, create a PR via GitHub:**

   1. Go to GitHub and create a Pull Request
   2. From: `release/1.2.0`
   3. To: `main`
   4. Title: `chore: merge release/1.2.0 back to main`
   5. Description:
      ```
      ## Release Merge-Back

      Merging v1.2.0 release back to main to capture version metadata.

      - Version tagged: v1.2.0
      - Last-minute fixes from stabilization captured
      - GitVersion metadata will use release tag for next version calculation
      ```
   6. Wait for CI to pass (tests, build validation)
   7. Request review if required by branch protection
   8. Merge via GitHub UI

5. **Clean Up**

   After the PR is merged:

   ```powershell
   git switch main
   git pull origin main

   # Delete release branch (optional, may keep for reference/patch releases)
   git push origin --delete release/1.2.0
   git branch -d release/1.2.0
   ```

### Why Merge Back to Main After Tagging?

This approach ensures:
- ✅ Captures version metadata and release commit in main's history
- ✅ Any last-minute fixes from the release branch are in main
- ✅ `GitVersion` uses the release tag to inform the next version number
- ✅ Keeps main as the superset of all released versions

### Branch States During Release

| Phase | `main` State | `release/1.2.0` State |
| --- | --- | --- |
| Before release | **Alpha** (development) | Doesn't exist |
| Release branch created | **Alpha** (continues new features) | **Beta** (stabilization only) |
| After tag | **Alpha** (new dev work) | **Beta** (maintenance only) |
| After merge back | **Includes** tagged commit | Can be deleted |

### Post-Release

1. **Monitor Stability**
   - Monitor error rates and user-reported issues
   - Be ready to cut a hotfix if critical issues are found

2. **Critical Hotfixes** (see Hotfix Workflow section below)
   - If issues are found in production, create hotfixes
   - These may need to be applied to multiple branches

3. **Next Release Planning**
   - Update the release plan for the next version
   - Review what worked and what didn't

---

## Hotfix Workflow (Emergency Production Fixes)

For critical production issues discovered after release:

### Scenario 1: Bug Found in Current Release

If a bug is found in a released version while the release branch still exists:

1. **Create feature branch from release branch**

   ```powershell
   git switch release/1.2.0
   git pull origin release/1.2.0
   git switch -c feature/hotfix-critical-issue
   ```

2. **Implement, test, and create PR to release branch**

   ```powershell
   # ... fix and test ...
   git push -u origin feature/hotfix-critical-issue
   # Create PR targeting release/1.2.0 (not main)
   ```

3. **After merge to release branch, cherry-pick to main**

   ```powershell
   git switch main
   git pull origin main
   git cherry-pick [commit-hash]
   git push origin main
   ```

   This ensures the fix is in both the released version and future development.

### Scenario 2: Bug Found in Development (main)

For bugs found in code that hasn't been released yet:

1. **Create feature branch from main**

   ```powershell
   git switch main
   git pull origin main
   git switch -c feature/hotfix-issue-name
   ```

2. **Implement, test, and create PR to main**

   ```powershell
   # ... fix and test ...
   git push -u origin feature/hotfix-issue-name
   # Create PR targeting main
   ```

3. **If release branch exists and bug affects it, cherry-pick**

   ```powershell
   git switch release/1.2.0
   git pull origin release/1.2.0
   git cherry-pick [commit-hash]
   git push origin release/1.2.0
   ```

### Critical Hotfix to Production (No Release Branch)

If a critical issue is in production and the release branch has already been deleted:

1. **Create feature branch from the release tag**

   ```powershell
   git switch --detach v1.2.0
   git switch -c feature/hotfix-production-critical
   ```

2. **Implement, test, and push**

   ```powershell
   # ... fix and test ...
   git push -u origin feature/hotfix-production-critical
   ```

3. **Create PRs to both main and create a new patch release branch**

   ```powershell
   # PR to main (for future versions)
   # Then create new patch release for production
   git switch main
   git pull origin main
   git switch -c release/1.2.1
   git cherry-pick [hotfix-commit-hash]
   git push -u origin release/1.2.1
   # Follow release process to tag and deploy v1.2.1
   ```

### Best Practices for Hotfixes

- ✅ Keep changes minimal and focused
- ✅ Add a regression test
- ✅ Test thoroughly before deploying
- ✅ Document the issue and fix clearly
- ✅ Notify stakeholders of the fix availability
- ✅ Ensure hotfixes are available in all affected branches

---

## Release Documentation

### Generate Release Notes

Before publishing a release:

1. Collect commit messages and PR titles from `main` since last release
2. Group by type (features, fixes, refactoring, docs, etc.)
3. Create or update `RELEASE_NOTES.md` or GitHub release draft
4. Include:
   - Summary of major features
   - List of bug fixes
   - Known issues or limitations
   - Upgrade/migration notes (if applicable)

### Tag and Publish

```powershell
# Tag (already done when stabilizing release branch)
git tag v1.2.0
git push origin v1.2.0

# Publish GitHub Release
# Use the commit hash of the tag for the release notes
```

---

## Deployment Targets

### Development Environment

- Runs latest `main` branch code
- Automatic deployment on every merge
- Used for continuous testing and validation

### Staging Environment

- Runs release candidate or specific release version
- Manual or scheduled deployment
- Used for QA and user acceptance testing

### Production Environment

- Runs released, tagged version
- Manual deployment with approval
- Triggered by release workflow or manual trigger

---

## Environment-Specific Configuration

Configuration and secrets management:

- `src/BookShelves.ServiceDefaults` — Shared service defaults
- `.github/workflows/*.yml` — Environment variable mapping
- GitHub Secrets — Production credentials and configuration

**Never commit secrets or connection strings.** Use:
- GitHub Secrets for CI/CD
- User Secrets for local development (`.dotnet user-secrets`)
- Environment variables in production

---

## Release Checklist

Before publishing a release:

- [ ] `main` is stable (all tests pass, no known critical bugs)
- [ ] Version number updated in `src/Directory.Build.Props`
- [ ] Commit messages and PR titles are clear
- [ ] Release notes are generated and complete
- [ ] `CHANGELOG.md` (if used) is updated
- [ ] No merge conflicts in release branch
- [ ] Code review and sign-off from maintainers
- [ ] CI pipeline passes for release commit
- [ ] Git tag created with proper version format
- [ ] Release published to GitHub Releases
- [ ] Deployment triggers are configured correctly
- [ ] Stakeholders are notified

---

## Versioning and Release Documentation

- See `docs/Versioning-and-Release.md` for detailed versioning and build metadata information
- See `.github/RELEASE_PROCESS.md` for workflow trigger details and environment mapping
- See `.github/workflows/` for CI/CD pipeline definitions

---

## Common Questions

**Q: How often are releases cut?**  
A: (TBD — Define your release cadence: daily, weekly, monthly, etc.)

**Q: Can I merge directly to `main` or do I need a release branch?**  
A: Features merge directly to `main` via PR. Release branches are optional and used only for release stabilization or hotfixes.

**Q: What if I need to push a fix for version 1.5 while 1.6 is in development?**  
A: Use a `hotfix/` branch, merge to `main`, and release the fix as patch version (e.g., 1.5.1). Then ensure the fix is in 1.6.

**Q: How do I know what version is currently deployed?**  
A: Check the API version endpoint or review the deployed release tag. Version information is also available in application logs.

**Q: Can I rollback a release?**  
A: Yes. Deploy the previous release version. Any issues found should be fixed in a new version based on the current `main`.

