# WI-002: Define Action Types and Validation Rules

## Epic
Epic 1 — Domain + Contracts

## Priority
P0

## Status
Completed

## Goal
Define allowed action types and enforce validation rules consistently across API contracts and processing.

## Scope
- Add action type enum/constants (example: `ToBeRead`, `PagesRead`, `Finished`).
- Define input rules:
  - `BookId` required.
  - `ActionType` required and must be supported.
  - `StartTimeUtc` and `EndTimeUtc` must be set (server may normalize/overwrite to UTC).
  - `EndTimeUtc` must not be earlier than `StartTimeUtc`.
  - `Details` is required and must match the record subtype for the action type.
  - `PagesRead` must be non-negative for page-progress records.
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