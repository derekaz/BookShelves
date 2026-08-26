# WI-002: Define Action Types and Validation Rules

## Epic
Epic 1 — Domain + Contracts

## Priority
P0

## Status
Not Started

## Goal
Define allowed action types and enforce validation rules consistently across API contracts and processing.

## Scope
- Add action type enum/constants (example: `ToBeRead`, `PagesRead`, `Finished`).
- Define input rules:
  - `BookId` required.
  - `ActionType` required and must be supported.
  - `OccurredAtUtc` must be set (server may normalize/overwrite).
  - `PagesRead` required for page-progress actions and must be non-negative.
- Ensure model validation integrates with current API validation behavior.

## Out of Scope
- Business analytics/projection tables.
- MAUI offline conflict behavior.

## Dependencies
- WI-001

## Acceptance Criteria
- Unsupported action types are rejected.
- Invalid payloads return expected validation failures.
- Page-related actions enforce page rules.
- Validation behavior is documented in the item and reflected in API tests (WI-011).

## Notes
Validation should stay minimal for MVP; avoid introducing rules that diverge from existing data flow patterns.