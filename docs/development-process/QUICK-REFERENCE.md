# Quick Reference Card

A one-page reference for common tasks and governance.

---

## Starting a Feature

```bash
git switch -c feature/[feature-name]
# Code, test, commit
git commit -m "feat: description"
git push -u origin feature/[feature-name]
# Create PR to main via GitHub
```

**Review checklist before requesting:**
- [ ] Code compiles, no warnings: `dotnet build`
- [ ] All tests pass: `dotnet test`
- [ ] Code formatted: `dotnet format`
- [ ] Authorization checks in place
- [ ] Tests added for new logic
- [ ] Docs updated
- [ ] Branch up-to-date with main

---

## Preparing a Release

```bash
# 1. Create release branch from main (stabilization only)
git switch main
git pull origin main
git switch -c release/1.2.0
git push -u origin release/1.2.0

# 2. Fix critical issues on release branch (no new features!)
# Create feature branches off release/1.2.0 if needed

# 3. Tag the release (final step)
git tag v1.2.0
git push origin v1.2.0

# 4. Merge back to main via PR (main is protected)
# Create a PR on GitHub:
#   From: release/1.2.0
#   To: main
#   Title: chore: merge release/1.2.0 back to main
# After PR is approved and CI passes, merge via GitHub UI

# 5. Clean up
git push origin --delete release/1.2.0  # optional, after PR is merged
```

**Note**: Step 4 requires a PR because `main` is a protected branch. You cannot push directly.

- `main` = Alpha (development)
- `release/1.2.0` = Beta (stabilization)
- `v1.2.0` tag = Production ready

---

## Branch Structure

```
main (Alpha)           ← Features merge here
  └── release/1.2.0 (Beta)
      └── Tag v1.2.0
          └── Merge back to main
```

---

## Code Standards (1-minute recap)

| Aspect | Standard |
| --- | --- |
| Classes, types | `PascalCase` |
| Variables, params | `camelCase` |
| Namespace | Match folder structure |
| Indentation | 4 spaces |
| Access modifiers | Always explicit |
| Types | Use `string`, `int`, not `System.String` |
| Async | Use `async`/`await` for I/O |

---

## Architecture Layers

```
API / Web / MAUI (Hosts)
		↓
Service Layer (Business logic)
		↓
Data Abstractions (Repositories)
		↓
Shared Models & Infrastructure

⚠️ Never: Shared depends on Host
```

---

## Test Naming

```
MethodName_Condition_ExpectedResult

✅ GetUserBooks_WithValidUser_ReturnsBooks
✅ CreateAction_WithUnauthorizedUser_ReturnsForbidden
```

---

## Definition of Done Checklist

```
Code       [ ] Follows conventions  [ ] No layer violations  [ ] Async correct
Auth       [ ] Checks in place      [ ] Data isolation OK    [ ] Admin override tested
Testing    [ ] Unit tests added     [ ] All tests pass       [ ] In right project
Docs       [ ] Updated              [ ] Comments explain why
Build      [ ] Compiles             [ ] No warnings          [ ] Formatted
Quality    [ ] Up-to-date with main [ ] Ready to review
```

---

## Common Commands

```powershell
# Build and test
dotnet build "BookShelves (Maui, Web and WebApi).slnx"
dotnet test "BookShelves (Maui, Web and WebApi).slnx"
dotnet test test/BookShelves.WebApi.Tests/BookShelves.WebApi.Tests.csproj

# Format code
dotnet format "BookShelves (Maui, Web and WebApi).slnx" --no-restore --exclude Templates/src --exclude-diagnostics CA1822

# Git
git switch -c feature/[name]      # Create feature branch
git push -u origin feature/[name] # Push and set upstream
git pull origin main              # Update with latest
```

---

## Decision Tree

**Where does this code go?**

```
Is it UI logic?
├─ YES → src/BookShelves.Shared (if cross-platform)
│        or host-specific folder (Web, Maui, etc)
└─ NO → Next question

Is it API endpoint logic?
├─ YES → src/BookShelves.WebApi
└─ NO → Next question

Is it data persistence?
├─ YES → src/BookShelves.Maui.Data
└─ NO → Service layer (in relevant host or Shared)
```

---

## Test Organization

```
test/BookShelves.Shared.Tests/          → Shared models, services, UI
test/BookShelves.WebApi.Tests/          → API endpoints, auth, contracts
test/BookShelves.Web.Tests/             → Web host endpoints
test/BookShelves.Web.Client.Tests/      → WebAssembly client services
test/BookShelves.Web.Shared.Tests/      → Web DTO/mapping
test/BookShelves.Maui.Data.Tests/       → MAUI data, sync
```

---

## Governance Philosophy

1. **Written > Oral** — Check docs before asking
2. **Accessible > Restrictive** — Standards are meant to help
3. **Enforceable > Aspirational** — CI validates everything it can
4. **Evolving > Fixed** — Update docs as needs change
5. **Intentional > Accidental** — Decisions via ADRs, not drift

---

## Need Help?

| Question | Where to Look |
| --- | --- |
| How do I work with branches? | `docs/development-process/branching-and-pr-strategy.md` |
| What are the coding standards? | `docs/development-process/code-standards-and-conventions.md` |
| What does "done" mean? | `docs/development-process/definition-of-done.md` |
| How do I test? | `docs/development-process/testing-standards.md` |
| Why did we choose X? | `docs/architecture/adr-index.md` |
| Where does this code go? | `docs/Solution-Structure.md` |
| How do I release? | `docs/development-process/release-process.md` |

---

## Key ADRs (Why We Chose What)

| ADR | Decision |
| --- | --- |
| ADR-0001 | Use Blazor for cross-platform UI |
| ADR-0002 | Per-user data isolation (privacy by default) |
| ADR-0003 | Async/await for all I/O (scalability) |
| ADR-0004 | Layered architecture (clean dependencies) |
| ADR-0005 | Entity Framework Core (consistent ORM) |
| ADR-0006 | Aspire for local dev (service orchestration) |
| ADR-0007 | Test ownership by layer (clear responsibility) |
| ADR-0008 | Policy-based authorization (flexible roles) |

---

## Pro Tips

💡 **Before coding**: Check if similar code exists elsewhere  
💡 **Before testing**: Use existing test builders and helpers  
💡 **Before committing**: Run build, format, and tests locally  
💡 **Before PR**: Verify Definition of Done is met  
💡 **Before merge**: Get code review from another set of eyes  

---

*See `docs/GOVERNANCE-SUMMARY.md` for the full story.*
