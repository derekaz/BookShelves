# Repository Governance and Standards

This folder contains the formal governance documents for the BookShelves repository: branching strategy, coding standards, quality gates, and architectural decisions.

---

## Quick Start

**New contributor?** Start here:

1. Read `docs/README.md` for the documentation index
2. Read `docs/Solution-Structure.md` to understand the project layout
3. Read `development-process/branching-and-pr-strategy.md` to understand how to work with branches
4. Read `development-process/code-standards-and-conventions.md` for coding guidelines
5. Read `development-process/definition-of-done.md` to understand quality expectations

---

## Documents in This Folder

### Development Process

These documents define *how* we work:

- **`branching-and-pr-strategy.md`** — How to create feature branches, write commits, and submit PRs
  - Branch naming and structure
  - Feature scope (2-5 days, ~300-500 lines)
  - PR workflow and size guidelines
  - Commit message conventions

- **`code-standards-and-conventions.md`** — How we write code
  - C# naming conventions and style (from `.editorconfig`)
  - Project structure and layering rules
  - Architectural patterns (DI, async/await, authorization)
  - Testing and documentation expectations
  - Code review standards

- **`definition-of-done.md`** — What "done" means
  - Feature completion criteria
  - Bug fix and refactoring expectations
  - Quality gates and test requirements
  - Code review checklist for reviewers

- **`release-process.md`** — How features move to production
  - Versioning strategy (from `GitVersion.yml` and `Directory.Build.Props`)
  - Release workflow and candidate testing
  - Hotfix procedures
  - Deployment targets and configuration

### Architecture

These documents explain *why* we designed things the way we did:

- **`../architecture/adr-index.md`** — Architecture Decision Records (ADRs)
  - ADR-0001: Blazor for cross-platform UI
  - ADR-0002: Per-user data isolation
  - ADR-0003: Async/await throughout
  - ADR-0004: Layered architecture
  - ADR-0005: Entity Framework Core
  - ADR-0006: Aspire for local dev
  - ADR-0007: Test ownership
  - ADR-0008: Policy-based authorization
  - Guidelines for proposing new ADRs

---

## Governance Layers

### 1. Strategic (Architecture Level)

**Owned by:** Architecture ADRs  
**Frequency of change:** Rare (major version changes)  
**Examples:**
- Blazor over native UI
- Async/await patterns
- Layered architecture

### 2. Tactical (Code Organization Level)

**Owned by:** Code Standards, Solution Structure  
**Frequency of change:** Occasional (quarterly reviews)  
**Examples:**
- Naming conventions
- Project structure
- Dependency rules

### 3. Operational (Daily Development Level)

**Owned by:** Branching Strategy, Definition of Done  
**Frequency of change:** As needed (iteration-based)  
**Examples:**
- Branch naming
- PR review criteria
- Test ownership

### 4. Quality (Enforcement Level)

**Owned by:** CI/CD pipelines, code analysis  
**Frequency of change:** As tooling evolves  
**Examples:**
- Build must pass
- Tests must pass
- Format must validate

---

## Decision Rights and Escalation

### Who Can Decide What?

| Decision Type | Authority | Process |
| --- | --- | --- |
| Feature scope and priority | Project owner | Work item estimation |
| Code standards and conventions | Team consensus | Code review + ADR if architectural |
| Architectural patterns | Architecture review | ADR proposal and approval |
| Test coverage expectations | Testing strategy owner | Update `Testing-Strategy.md` and ADRs |
| Release timing and scope | Release manager | Release planning meeting |
| Process and workflow changes | Team consensus | Update relevant `development-process/` docs |

### How to Propose a Change

1. **Small or tactical changes** (code style, naming, documentation):
   - Discuss in PR or team meeting
   - Update relevant doc
   - No formal process needed

2. **Medium changes** (test strategy, process workflow):
   - Create issue or PR with proposal
   - Document rationale
   - Discuss with team
   - Update `development-process/` docs and ADRs

3. **Major changes** (architecture, new platform, data model):
   - Create an ADR (see `../architecture/adr-index.md`)
   - Discuss with technical leads
   - Implement with broad team input
   - Update `Solution-Structure.md` and related docs

---

## Consistency and Enforcement

### Automated Enforcement

These are checked automatically by CI:

- ✅ `.editorconfig` — Formatting and style
- ✅ `dotnet format` — Automatic formatting
- ✅ Code analyzers — StyleCop, CA rules
- ✅ Test suites — Unit, integration, contract tests
- ✅ Build validation — No warnings or errors

### Manual Enforcement (Code Review)

These are checked by human reviewers:

- ✅ Architecture and layering (no circular dependencies)
- ✅ Authorization and data isolation
- ✅ Test coverage and quality
- ✅ Documentation updates
- ✅ Commit message clarity
- ✅ PR scope (no speculative changes)

### Process Enforcement

These are enforced through documentation and team awareness:

- ✅ Branch strategy (naming, size)
- ✅ Definition of done (all criteria met)
- ✅ Release process (versioning, tagging)

---

## Review Schedule

To keep governance documents accurate and relevant:

| Document | Review Frequency | Owner |
| --- | --- | --- |
| `branching-and-pr-strategy.md` | Quarterly | Team |
| `code-standards-and-conventions.md` | Quarterly | Team |
| `definition-of-done.md` | Quarterly | Team |
| `release-process.md` | After each release | Release manager |
| `../architecture/adr-index.md` | As ADRs are added | Architecture review |
| `.editorconfig` | As needed | Team |

---

## Updating Governance Documents

1. **Identify the problem or need** — Document what's not working
2. **Propose a change** — Draft an update to the relevant document
3. **Discuss with team** — Get feedback before finalizing
4. **Update the document** — Use clear, concise language
5. **Communicate the change** — Make sure the team knows
6. **Enforce gradually** — Give time for adoption, don't break existing PRs

---

## Related Organizational Documents

**Solution-level documentation:**
- `docs/README.md` — Documentation index
- `docs/Solution-Structure.md` — Project layout and ownership
- `docs/Developer-and-AI-Guidance.md` — Contributor guardrails
- `docs/Testing-Strategy.md` — Test ownership and quality gates
- `docs/Build-Test-Run.md` — Build, test, and run commands

**CI/CD and Release:**
- `.github/workflows/` — Automated build and test pipelines
- `.github/RELEASE_PROCESS.md` — Workflow details and environment mapping
- `GitVersion.yml` — Version control strategy

**Code Configuration:**
- `.editorconfig` — Style and formatting rules

---

## Governance Philosophy

This repository follows these principles:

1. **Written > Oral** — Decisions and standards are documented, not tribal knowledge
2. **Accessible > Restrictive** — Documents are clear and available to all contributors
3. **Enforceable > Aspirational** — Standards are checked (automated where possible)
4. **Evolving > Fixed** — Governance adapts as the project and team grow
5. **Intentional > Accidental** — Changes happen through documented decisions, not drift

---

## Questions?

If you have questions about governance or standards:

1. Check the relevant document in this folder
2. Search `docs/` for related guidance
3. Ask in team meetings or PR discussions
4. Propose an update if something is unclear or outdated

