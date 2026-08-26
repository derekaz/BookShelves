# WI-010: Formalize Admin Policy Usage for User-Scoped Tracking

## Epic
Epic 5 — Authorization Policy Refinement

## Priority
P0

## Status
Not Started

## Goal
Ensure admin/non-admin behavior is explicit and consistent for user-book tracking without changing global books/authors access behavior.

## Scope
- Reuse `AuthorizationPolicies.AdminAccess` where applicable.
- Ensure API and server code paths apply admin checks consistently for cross-user scenarios.
- Document expected behavior:
  - non-admin => own user-book data only
  - admin => cross-user allowed
  - books/authors => remain global/shared
- Keep the policy guidance aligned with the action-record contract and security rules around user-owned entries that contain `StartTimeUtc`, `EndTimeUtc`, `ActionType`, and metadata fields.

## Out of Scope
- New role taxonomy.
- Identity provider setup changes.

## Dependencies
- WI-004

## Acceptance Criteria
- Authorization behavior is deterministic across read/write operations.
- No accidental policy changes to existing global books/authors functionality.
- Behavior is testable through WI-011 and WI-012.

## Notes
This item is governance and consistency focused; keep policy usage centralized and minimal.