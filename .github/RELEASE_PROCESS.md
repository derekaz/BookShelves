Release branch and CI/CD process
================================

Branch naming and intent
- main: protected trunk. Merges to main run validation only (no automatic publish).
- release/<MAJOR>.<MINOR>.<PATCH>: release branch for a specific semantic series (example: release/0.1.6).
- feature/<name>: feature branches. Feature branch pushes run validation automatically; publishing full artifacts from feature branches is manual.

Per-release build numbering
- The CI computes a per-release build number for each build and exposes it as needs.validate.outputs.BuildNumber.
- Calculation method: for a given semantic version (from GitVersion majorMinorPatch), the workflow finds the remote release/<version> branch and computes the number of commits since the merge-base between that release branch and the current HEAD. That count increments across commits and PRs that are based on the release branch and remains stable when merged to main. When you start a new release/<x> branch, the counter resets for that new series.
- Fallback: if no release/<version> branch is found, the workflow falls back to GitVersion's commitsSinceVersionSource value.

How to start a new release series
1. Create a branch named release/X.Y.Z (example: release/0.1.6).
2. Push the branch to origin.
3. Build/publish on that branch will compute build numbers starting from 0 and increasing for each commit/PR.

Release creation and final publish
- When you are ready to publish a final release, create a GitHub Release from the appropriate commit (tag name e.g. v0.1.6). The workflow will run on the release event and compute the same BuildNumber for the published artifact.

Manual approvals for publish jobs and environment mapping
- Publish jobs (containers, Apple, Windows) are assigned an environment dynamically based on the build context:
  - production: when the workflow runs for a GitHub Release event (official tagged release). This should be created from the main branch for production releases.
  - staging: when the workflow runs on main but is not a Release event (for example, merges to main that are not the official release tag).
  - development: for builds on release/* branches and feature/* branches.

  The workflow sets the environment automatically; you must configure environment protection rules in the repository settings (Settings -> Environments -> <environment-name>) to require reviewers or wait timers. Approvers configured for the environment will be prompted to approve deployments to that environment.

Note: configuring environment protection is required via GitHub UI by a repository admin — the workflow only assigns the job to the computed environment name.

Feature branch publishing
- Feature branch pushes run validation automatically. If you need to publish a package from a feature branch, trigger a manual workflow_dispatch or use the Release pipeline and select the appropriate artifacts.

Using the BuildNumber in MSBuild
- The validate job exposes:
  - DisplayVersion: semantic version (major.minor.patch)
  - BuildNumber: per-release build number
  - FullSemVer: full semantic version

Example MSBuild properties used in workflow (already present):
- /p:ApplicationDisplayVersion="${{needs.validate.outputs.DisplayVersion}}"
- /p:ApplicationVersion=${{needs.validate.outputs.BuildNumber}}

Questions or changes
- If you want the release environment to have a different name or multiple environments (staging, production), tell me and I will update the workflow accordingly.
