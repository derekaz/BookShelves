# User Book Action Tracking — Slice Roadmap

This folder tracks local planning and execution for user-book action tracking.

## Core Requirements
- Follow existing online/offline data processes and familiar sync patterns.
- Keep `Books` and `Authors` global/shared.
- Enforce user isolation for user-book tracking data:
  - non-admin users: own related data only
  - admin users: broader access by policy
- Use the existing Cosmos DB shared container.

---

## Slice Summary

### Slice A (MVP, P0)
**Focus:** WebApi + Web server foundations with security and tests.  
**Includes:** Epics 1, 2, 3, 5, 6.  
**Docs:** [`./slice-a/README.md`](./slice-a/README.md)

### Slice B (P1)
**Focus:** Blazor UI integration for book action capture and visibility.  
**Includes:** Epic 4.  
**Docs:** [`./slice-b/README.md`](./slice-b/README.md)

### Slice C (Later)
**Focus:** MAUI/local offline parity and sync alignment for mobile/desktop host.  
**Includes:** MAUI parity follow-up work.  
**Docs:** [`./slice-c/README.md`](./slice-c/README.md)

---

## Cross-Slice Dependencies
- Slice B depends on Slice A API/service/security contracts.
- Slice C depends on Slice A contract stability and should align with Slice B UI behavior where applicable.

---

## Execution Guidance
- Execute Slice A first end-to-end (including tests).
- Start Slice B after Slice A APIs are stable.
- Schedule Slice C when MAUI parity is prioritized.
