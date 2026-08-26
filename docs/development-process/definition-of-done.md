# Definition of Done

This document defines what "done" means for features, bug fixes, and refactoring work in the BookShelves repository.

---

## Feature Implementation

A feature is **done** when all of the following criteria are met:

### Code and Functionality

- [ ] Feature is implemented and working as specified
- [ ] Code follows `docs/development-process/code-standards-and-conventions.md`
- [ ] No circular dependencies or architectural layer violations
- [ ] Async/await patterns are correct throughout
- [ ] Error handling and validation are in place
- [ ] Configuration and secrets are managed securely

### Authorization and Data Isolation

- [ ] Authorization checks are enforced at endpoint/service boundaries
- [ ] Per-user data isolation is validated (if applicable)
- [ ] Admin overrides are tested and documented (if applicable)
- [ ] No unintended exposure of user-scoped data

### Testing

- [ ] Unit tests added for new logic
- [ ] Integration tests added for cross-layer behavior
- [ ] Contract tests added for API endpoints (if applicable)
- [ ] All tests pass locally and in CI
- [ ] Test coverage aligns with `docs/Testing-Strategy.md` ownership map
- [ ] No regression in existing test suite

### Documentation

- [ ] Solution-level docs updated (if behavior, workflow, or architecture changed)
- [ ] Inline comments explain non-obvious logic
- [ ] Public API has XML doc comments
- [ ] Commit messages are clear and reference issue/work item (if applicable)

### Build and Quality

- [ ] `dotnet build` succeeds with no warnings
- [ ] `dotnet format` applied and passes validation
- [ ] All test projects pass: `dotnet test "BookShelves (Maui, Web and WebApi).slnx"`
- [ ] Branch is up-to-date with `main`
- [ ] No merge conflicts

### Pull Request

- [ ] PR description clearly explains the change and why
- [ ] PR title follows commit message conventions (e.g., `feat:`, `fix:`, `refactor:`)
- [ ] Related issue/work item is linked (if applicable)
- [ ] No speculative changes unrelated to the feature
- [ ] Ready for code review and merge

---

## Bug Fix Implementation

A bug fix is **done** when all feature criteria are met, plus:

### Bug-Specific

- [ ] Root cause is identified and fixed (not worked around)
- [ ] Regression test added that would fail before the fix
- [ ] Regression test passes after the fix
- [ ] Related bugs or similar issues are checked for (preventive sweep)
- [ ] Fix has been validated in the actual affected layer (not just tests)

### Documentation

- [ ] Bug is documented in commit message or PR description
- [ ] Workarounds are removed if the root fix makes them unnecessary
- [ ] If environmental/setup-related, add a note to `docs/Build-Test-Run.md` (if needed)

---

## Refactoring Work

Refactoring is **done** when:

### Scope and Impact

- [ ] Refactoring is explicitly scoped and requested (no speculative cleanup)
- [ ] Behavior is unchanged before and after (characterized by tests)
- [ ] No new features are introduced in the refactoring PR
- [ ] Performance impact (if any) is measured and acceptable

### Code Quality

- [ ] Code follows standards and conventions
- [ ] All existing tests pass without modification
- [ ] No new technical debt introduced
- [ ] Readability is improved or at least maintained

### Documentation

- [ ] If architecture or organization changed, update `docs/Solution-Structure.md`
- [ ] PR clearly explains why the refactoring was necessary
- [ ] Commit messages describe what was changed

---

## Architecture or Process Changes

Changes to architecture, process, or infrastructure are **done** when:

### Planning and Review

- [ ] Decision was made as an ADR (Architecture Decision Record) or explicitly documented
- [ ] Rationale for the change is clear and documented
- [ ] Impact on existing code and processes is assessed
- [ ] Stakeholders have reviewed and approved

### Implementation

- [ ] All code follows the new patterns
- [ ] Existing code is updated to align (if not deferred to follow-up)
- [ ] Migration plan is clear if multiple systems are affected

### Documentation

- [ ] `docs/Developer-and-AI-Guidance.md` updated if process changed
- [ ] `docs/Solution-Structure.md` updated if architecture changed
- [ ] `docs/Testing-Strategy.md` updated if testing approach changed
- [ ] Related guidance docs are cross-linked

---

## Code Review Checklist (for Reviewers)

Before approving a PR, verify:

### Functional Correctness

- [ ] Behavior matches the PR description
- [ ] Tests are appropriate and pass
- [ ] Error handling is present and correct
- [ ] Authorization/data isolation is correct (if applicable)

### Code Quality

- [ ] Follows naming, style, and formatting conventions
- [ ] No circular dependencies or layer violations
- [ ] Async/await patterns are correct
- [ ] Comments explain *why*, not *what*
- [ ] No speculative changes outside scope

### Testing and Coverage

- [ ] Test project matches `docs/Testing-Strategy.md` ownership
- [ ] Tests are deterministic and not flaky
- [ ] Coverage is appropriate for the layer (unit/integration/contract)
- [ ] Bug fixes include regression tests

### Documentation

- [ ] PR description is clear and complete
- [ ] Commit messages are atomic and well-written
- [ ] Docs are updated where necessary
- [ ] No outdated or misleading comments

### CI and Build

- [ ] All CI checks pass (build, tests, format, analysis)
- [ ] No new warnings introduced
- [ ] Branch is up-to-date with `main`

---

## Quality Gates (Phase 3)

These gates are automatically enforced:

1. ✅ All test projects in the solution pass before merge
2. ✅ Pull request validation executes core test projects in CI
3. ✅ CI publishes test evidence (TRX and coverage artifacts)
4. ✅ Every production bug fix includes a regression test
5. ✅ New test projects are added under `test/` and documented

---

## Related Documentation

- `docs/development-process/branching-and-pr-strategy.md` — How to create and manage PRs
- `docs/development-process/code-standards-and-conventions.md` — Coding standards
- `docs/Testing-Strategy.md` — Test ownership and quality expectations
- `docs/Solution-Structure.md` — Project organization and responsibilities

