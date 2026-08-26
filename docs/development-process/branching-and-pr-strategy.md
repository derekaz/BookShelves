# Branching and PR Strategy

## Overview

This document outlines the branching and pull request process for the BookShelves project.

**TL;DR**: 
- **Feature development**: Branch from `main` using `feature/[feature-name]`, keep PRs focused, aim for 2-5 days per branch
- **Releases**: Create `release/[major.minor.patch]` from `main` for stabilization and final changes
- **Version states**: `main` is alpha (pre-release), `release/` is beta, tagged versions are production-ready
- **Merge flow**: Features → main, then later create release branch, stabilize, tag, then merge release back to main

---

## Branch Structure

```
main (alpha — integration branch for next release)
  ├── feature/user-book-list
  ├── feature/user-book-details
  ├── feature/user-book-edit
  ├── feature/user-book-delete
  ├── feature/book-ratings
  └── feature/[other features]

release/[major.minor.patch] (beta — stabilization and last-minute changes)
  └── (merge back to main after tagging)
```

**Branch Purposes:**

- **`main`** — Alpha: Integration branch for features intended for the next release
  - All feature branches are created from and merge to `main`
  - Represents the latest development work
  - Not yet stabilized for production

- **`release/[major.minor.patch]`** — Beta: Stabilization branch for a specific release
  - Created from `main` when ready to prepare a release
  - Only stabilization and critical last-minute fixes go here
  - No new features should be added to release branches
  - After tagging with final version number, merged back to `main` to capture release metadata

**Rules:**
- Feature branches branch from `main`, merge back to `main` via Pull Request
- Release branches are created from `main` when a release is being prepared
- Only critical bug fixes and stabilization changes in release branches
- Release branches are merged back to `main` AFTER tagging (to capture version metadata)
- Branch names should be descriptive and lowercase (kebab-case)

---

## Feature Branch Scope

### What Makes a Good Feature Branch?

A feature branch should contain **one logical feature** that:
- Represents a complete, user-facing capability (or backend equivalent)
- Takes approximately 2-5 days of development
- Results in a PR reviewable in 15-30 minutes
- Changes approximately 300-500 lines of code (soft guideline, not strict)

### Examples of Good Feature Scope

✅ `feature/user-book-list` — Display user's book collection with filtering  
✅ `feature/user-book-details` — View full details of a single book  
✅ `feature/user-book-edit` — Update book information  
✅ `feature/book-ratings` — Add and display book ratings  
✅ `feature/library-creation` — Create a new library (backend + UI)  

### When to Split Work

If a feature branch is becoming too large:
- Split into multiple independent `feature/` branches
- Example: `feature/library-creation` (backend) + `feature/library-ui` (frontend)
- Each branch can merge to `main` independently

### When to Combine Work

If a feature is genuinely small:
- Bundle related work into a single branch
- Example: `feature/user-book-list` might include search functionality

---

## Workflow

### Creating a Feature Branch

```powershell
# Switch to main and ensure it's up-to-date
git switch main
git pull origin main

# Create and switch to feature branch
git switch -c feature/[feature-name]
```

### During Development

```powershell
# Make changes, commit regularly with clear messages
git add .
git commit -m "feat: add book list filtering"
git commit -m "feat: add sorting by title"
git commit -m "test: add tests for filtering"

# Push to remote (first push uses -u to set upstream)
git push -u origin feature/[feature-name]
```

### Creating a Pull Request

1. Push all commits to the remote branch
2. Go to GitHub and create a Pull Request
3. Target: `main` (base), your feature branch (compare)
4. Include:
   - Clear description of what the feature does
   - Related issue/work item (if applicable)
   - Testing notes or deployment considerations
5. Request review from teammates (if applicable)

### After Review

```powershell
# Address review feedback with additional commits
git add .
git commit -m "refactor: improve error handling feedback"
git push origin feature/[feature-name]

# Once approved, merge via GitHub (prefer "Squash and merge" or "Create a merge commit")
# Then clean up local branch
git switch main
git pull origin main
git branch -d feature/[feature-name]
```

---

## Commit Message Guidelines

Use conventional commits for clarity:

```
feat: add user book list view
fix: resolve null reference in book detail
refactor: simplify filtering logic
test: add unit tests for book service
docs: update API documentation
```

Format: `[type]: [description]`

Types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`

---

## PR Best Practices

### Size Guidelines

| Size | Guideline | Action |
|------|-----------|--------|
| < 100 lines | Likely too small | Combine with related work |
| 100-500 lines | **Ideal** | Ready to review |
| 500-1000 lines | Getting large | Consider if it's really one feature |
| > 1000 lines | Too large | Split into multiple branches |

### Description Template

```markdown
## Description
Brief explanation of what this PR does.

## Related Issue
Closes #[issue-number] (if applicable)

## Testing
How was this tested? Manual testing, unit tests, etc.

## Checklist
- [ ] Code follows project style guidelines
- [ ] Self-review completed
- [ ] Unit tests added/updated
- [ ] Documentation updated
```

### Review Expectations

- PRs should be reviewed within 24 hours
- Reviewers should focus on:
  - Code correctness and logic
  - Test coverage
  - Adherence to coding standards
  - Performance implications
- Authors should respond to feedback promptly

---

## Release Branch Workflow

### Overview

When preparing a release, a stabilization branch is created to separate last-minute changes from ongoing feature development.

### Creating a Release Branch

```powershell
# Switch to main and ensure it's up-to-date
git switch main
git pull origin main

# Create release branch with planned version
git switch -c release/1.2.0
git push -u origin release/1.2.0
```

### Release Branch Stability

**On the release branch:**
- ✅ Only critical bug fixes and stabilization changes
- ✅ No new features
- ✅ Comprehensive testing and validation
- ⚠️ Main continues to evolve with new features in parallel
- ⚠️ Features merged to main do NOT automatically go to release branch

### Tagging and Artifact Creation

Once the release is ready:

```powershell
# Tag the release with clean semantic version
git tag v1.2.0
git push origin v1.2.0

# Create final release artifacts (NuGet packages, app builds, etc.)
# This is the last action on the release branch
```

### Merging Back to Main

**After tagging**, merge the release branch back to main to capture version metadata:

```powershell
# From the release branch
git switch release/1.2.0
git pull origin release/1.2.0

# Merge back to main via PR (required due to branch protection)
git switch main
git pull origin main
git merge release/1.2.0
git push origin main  # Will be blocked - use GitHub PR instead
```

**Since `main` is a protected branch, you must create a PR:**

1. Push the merge locally (or use GitHub's UI to create a PR)
2. Go to GitHub and create a Pull Request from `release/1.2.0` to `main`
3. PR title: `chore: merge release/1.2.0 back to main`
4. PR description should include:
   - Version released (e.g., "v1.2.0")
   - Why this merge is needed: "Captures version metadata and last-minute fixes for future development"
   - Any notable changes in the release branch
5. Wait for CI to pass and get required review
6. Merge via GitHub UI
7. After merge, clean up release branch (optional):
   ```powershell
   git push origin --delete release/1.2.0
   git branch -d release/1.2.0
   ```

### Why Merge Back to Main After Tagging?

This approach ensures:
- ✅ Version metadata from the release branch (version file updates, build metadata) influences future development
- ✅ Any last-minute fixes from the release branch are available in main
- ✅ Tagged commits are part of main's history
- ✅ Main is always a superset of all released versions
- ✅ `GitVersion` can use release tags to inform the next version number

### Version Number States

| Branch | State | Version Format |
| --- | --- | --- |
| `main` | **Alpha** (pre-release) | `1.2.0-alpha.1+commit` |
| `release/1.2.0` | **Beta** (pre-release) | `1.2.0-rc.1+commit` or similar |
| Tagged `v1.2.0` | **Release** (stable) | `1.2.0` |

---

## Handling Special Cases

### Hot Fixes

```powershell
# Branch from main, fix the issue, PR to main
git switch main
git pull origin main
git switch -c feature/hotfix-critical-bug

# ... fix, commit, push ...
# Create PR to main
# After merge, cherry-pick to any active release branches if needed
git switch release/[version]
git cherry-pick [commit-hash]
git push origin release/[version]
```

**Note**: If a release branch is active and the bug affects it, cherry-pick the fix to the release branch as well.

### Fixing Issues in Release Branches

If an issue is discovered during release stabilization:

```powershell
# Create a feature branch from the release branch
git switch release/1.2.0
git pull origin release/1.2.0
git switch -c feature/hotfix-release-issue

# ... fix, commit, push ...
# Create PR targeting the release branch (not main)
# After merge to release, cherry-pick back to main if still in development
git switch main
git cherry-pick [commit-hash]
```

This ensures stabilization fixes stay in the release branch but are also available in main for future development.

### Cross-Feature Coordination

If two features depend on each other:
1. Implement and merge the dependency first
2. Pull the dependency in the dependent branch: `git pull origin main`
3. Continue work in the dependent branch

### Rebasing vs. Merging

- **Default**: Use GitHub's merge button (creates merge commit)
- **Alternative**: Rebase locally if you want a linear history
- Don't force-push to shared branches

---

## Integration and Testing

- All PRs must pass CI/CD checks before merging
- Automated tests run on every push
- Manual testing checklist should be included in PR description
- Code review is required before merge

---

## Summary Checklist

Before pushing a feature branch for review:

- [ ] Branch created from latest `main`
- [ ] Feature is complete and working
- [ ] Code follows project style
- [ ] Unit tests added/updated
- [ ] All commits have clear messages
- [ ] No merge conflicts
- [ ] Pushed to remote and PR created
- [ ] PR description is clear and complete
