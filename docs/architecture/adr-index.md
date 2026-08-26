# Architecture Decision Records (ADRs)

Architecture Decision Records (ADRs) document significant architectural and design decisions made for the BookShelves project. Each decision is recorded with its context, rationale, and consequences.

---

## Format

Each ADR follows this structure:

```markdown
# ADR-NNNN: [Decision Title]

## Status
[Proposed | Accepted | Deprecated | Superseded]

## Context
What is the issue that we're seeing that motivates this decision or change?

## Decision
What is the change that we're proposing and/or doing?

## Rationale
Why is this the best choice given the context?

## Consequences
What becomes easier or more difficult to do and any risks introduced by this change?

## Alternatives Considered
What other options did we consider?

## Related ADRs
Links to related or superseded ADRs (if any).
```

---

## Current ADRs

### ADR-0001: Use Blazor for Cross-Platform UI

**Status**: Accepted

**Context**  
BookShelves needed a UI framework that could run on web, desktop (Windows/macOS), and mobile (Android/iOS) without maintaining multiple codebases.

**Decision**  
Use Blazor components as the shared UI layer, hosted by .NET MAUI (mobile/desktop) and ASP.NET Core Blazor Web (web).

**Rationale**
- Single C# codebase for UI logic
- Blazor components are reusable across MAUI and web hosts
- Strong .NET ecosystem and community support
- Shared business logic and models across platforms

**Consequences**
- ✅ Reduces code duplication
- ✅ Faster feature development across platforms
- ⚠️ Requires careful separation of host-specific concerns
- ⚠️ MAUI and web have different deployment and styling constraints

**Alternatives Considered**
- Native UI per platform (Swift, Kotlin) — more complex maintenance
- Flutter or React Native — different language ecosystem
- Web-only approach — limited to browser platform

---

### ADR-0002: Per-User Data Isolation for User-Book Actions

**Status**: Accepted

**Context**  
User book actions (e.g., "read", "wishlist", "currently reading") need to be tracked and scoped to individual users. The system also requires admin users to view/manage broader data across users.

**Decision**  
Enforce per-user data isolation at the authorization layer. Non-admin users can only view and update their own actions; admin users can override this restriction.

**Rationale**
- Ensures data privacy and security by default
- Admin override allows operational support and debugging
- Centralized authorization policy makes enforcement consistent
- Easier to audit and test than distributed checks

**Consequences**
- ✅ Strong data privacy guarantees
- ✅ Clear admin/user role separation
- ⚠️ Every query must validate user scope (performance consideration)
- ⚠️ Authorization logic must be tested thoroughly

**Alternatives Considered**
- Row-level security in database — vendor-specific, less testable
- Client-side filtering — insufficient for security
- No isolation — privacy risk

---

### ADR-0003: Async/Await Throughout for I/O Operations

**Status**: Accepted

**Context**  
BookShelves spans multiple deployment platforms: web (scalability-critical), mobile (battery/responsiveness-critical), and desktop (UI responsiveness-critical). I/O operations (database, network) should not block threads.

**Decision**  
Use async/await consistently for all I/O operations (HTTP, database, file access). Sync-to-async bridges are only used at platform entry points (e.g., MAUI event handlers).

**Rationale**
- Web: Enables handling more concurrent users with fewer threads
- Mobile: Keeps UI responsive and reduces battery drain
- Desktop: Prevents UI freezes during network or database operations
- Consistent across all platforms

**Consequences**
- ✅ Better scalability and responsiveness
- ✅ Resource efficiency across platforms
- ⚠️ Requires discipline — easy to accidentally block with `.Result` or `.Wait()`
- ⚠️ Some legacy or third-party libraries may only offer sync APIs

**Alternatives Considered**
- Sync-only approach — insufficient for mobile and web scalability
- Mixed sync/async — creates threading complexity and bugs

---

### ADR-0004: Layered Architecture with Clear Dependency Direction

**Status**: Accepted

**Context**  
As BookShelves grows, it needs a scalable structure that minimizes circular dependencies and makes it clear which projects depend on which others.

**Decision**  
Enforce a layered architecture:
```
API / Web / MAUI (Hosts)
	   ↓
Shared Services
	   ↓
Data Abstractions
	   ↓
Shared Models & Infrastructure
```

Hosts depend on shared layers. Shared layers never depend on hosts.

**Rationale**
- Clear dependency direction makes code navigation easier
- Prevents circular dependencies
- Shared layers can be reused and tested independently
- Easier to identify what needs to change when requirements shift

**Consequences**
- ✅ Testable in isolation
- ✅ Reusable components
- ⚠️ Requires discipline to maintain boundary enforcement
- ⚠️ May introduce small adapters or bridge classes

**Alternatives Considered**
- Monolithic project — harder to navigate and test
- Complete separation of concerns per platform — duplicates logic

---

### ADR-0005: Entity Framework Core for Persistence

**Status**: Accepted

**Context**  
BookShelves needs data persistence on both server (API queries, potential caching) and client (MAUI local sync). A single abstraction reduces learning curve and code duplication.

**Decision**  
Use Entity Framework Core for both MAUI local persistence and API-layer queries. Extend with Domain-Driven Design patterns (repositories, value objects) at the service layer.

**Rationale**
- Single ORM across platforms
- Strong LINQ support for queries
- Good migration tooling
- Rich ecosystem

**Consequences**
- ✅ Consistent data access patterns
- ✅ Good tooling for migrations
- ⚠️ EF Core abstractions can hide complexity (N+1 queries, etc.)
- ⚠️ Client/server schema differences must be carefully managed

**Alternatives Considered**
- Dapper — lighter weight but more manual mapping
- ADO.NET — more control, more boilerplate
- Realm (mobile-specific) — platform-specific, different abstraction

---

### ADR-0006: Aspire for Local Development Orchestration

**Status**: Accepted

**Context**  
Local development requires coordination between multiple services: Web host, Web API, MAUI, database, and optional caching/messaging. Manual startup is error-prone.

**Decision**  
Use .NET Aspire (`AppHost` project) to define and orchestrate the full local development environment. Aspire provides service discovery and health checks for free.

**Rationale**
- Single definition of all services and dependencies
- Automatic service discovery (eliminates hardcoded URLs)
- Health checks and startup order management
- Aspire dashboard for monitoring local services

**Consequences**
- ✅ Simplified local environment setup
- ✅ Self-documenting service topology
- ⚠️ Requires Aspire runtime (modern .NET only)
- ⚠️ Learning curve for Aspire-specific concepts

**Alternatives Considered**
- Docker Compose — works but less integrated with .NET tooling
- Manual startup scripts — error-prone and hard to maintain
- Environment-specific configuration files — fragile

---

### ADR-0007: Test Ownership and Layered Testing Strategy

**Status**: Accepted

**Context**  
As test coverage grows, it's unclear which tests belong where, what layers to test, and how to balance unit/integration/contract testing.

**Decision**  
Assign test ownership by layer and project:
- Unit tests in shared layer test projects
- Integration tests at host/feature boundaries
- Contract tests for API endpoints
- See `docs/Testing-Strategy.md` for detailed ownership map

**Rationale**
- Clear responsibility for test maintenance
- Tests are close to the code they verify
- Reduces duplicate test coverage
- Easier to onboard new contributors

**Consequences**
- ✅ Clear test ownership
- ✅ Maintainable test suite
- ⚠️ Requires discipline to keep tests in right projects
- ⚠️ Cross-layer behavior needs explicit boundary tests

**Alternatives Considered**
- Single monolithic test project — hard to navigate
- No test organization — test duplication and maintenance confusion

---

### ADR-0008: Policy-Based Authorization for Role-Driven Access Control

**Status**: Accepted

**Context**  
Authorization requirements evolve (admin vs. user, moderator roles, feature flags). Hardcoding authorization checks makes changes difficult and error-prone.

**Decision**  
Use ASP.NET Core's policy-based authorization (`[Authorize(Policy = "AdminOnly")]`) with custom authorization handlers for complex checks.

**Rationale**
- Decouples authorization logic from endpoints
- Policies can be centrally defined and updated
- Easier to test and audit
- Clear and declarative

**Consequences**
- ✅ Flexible and maintainable
- ✅ Easy to add new policies without changing endpoints
- ⚠️ Requires custom handlers for complex logic
- ⚠️ Easy to forget policy on a new endpoint (needs code review vigilance)

**Alternatives Considered**
- Role-based (`[Authorize(Roles = "Admin")]`) — inflexible
- Attribute-based access control (ABAC) — overkill for current needs
- Resource-based authorization — would require passing resource through auth pipeline

---

## Adding a New ADR

When proposing a new architectural or design decision:

1. **Create a new markdown file**: `docs/architecture/adr-NNNN-[title].md`
2. **Increment the sequence number** from the highest existing ADR
3. **Set status to "Proposed"**
4. **Fill out all sections** (Context, Decision, Rationale, Consequences, Alternatives)
5. **Submit for review** as part of the feature/design PR
6. **Update status to "Accepted"** once approved
7. **Link from this index** (`docs/architecture/ADRs.md`)

---

## Reviewing ADRs

- Does the decision solve the stated problem?
- Are consequences realistic and acceptable?
- Did we consider alternatives?
- Is the decision testable/verifiable?
- Does it align with existing ADRs?

---

## Superseding an ADR

When a decision is overturned:

1. Keep the old ADR with status = "Superseded"
2. Add a "Superseded by ADR-XXXX" link
3. Create a new ADR with updated decision
4. Document reasons for the change in the new ADR

This preserves decision history and explains the evolution of the architecture.

---

## Related Documentation

- `docs/development-process/code-standards-and-conventions.md` — Coding standards derived from ADRs
- `docs/Solution-Structure.md` — Project organization reflecting layered architecture (ADR-0004)
- `docs/Testing-Strategy.md` — Test ownership reflecting ADR-0007

