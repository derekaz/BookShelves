# Work Items and Implementation Tracking

This folder contains documented work items, epics, and implementation plans organized by feature area.

## Current Work

### User Book Action Tracking

The user book action tracking feature is being implemented in slices to minimize rework and enforce security constraints early.

- **`user-book-action-tracking/slice-a/`** — MVP scope (P0)
  - Status: In progress (feature/user-book-details branch)
  - Includes: Domain contracts, WebAPI datasync table, Web server service layer, authorization, tests
  - See: `user-book-action-tracking/slice-a/README.md` for work item index
  - See: `user-book-action-tracking/slice-a/00-execution-order-and-dependencies.md` for execution order

## How to Use Work Items

Each work item follows this structure:

```markdown
# [WI-NNN] - [Title]

## Description
What needs to be done.

## Acceptance Criteria
- [ ] Testable condition 1
- [ ] Testable condition 2

## Implementation Notes
- Key patterns or constraints
- Where code should go
- What to test

## Dependencies
- Related work items (if any)
```

## Adding New Work Items

1. Create a new markdown file: `[feature]/[slice]/epic-NN-[name]/WI-NNN-[title].md`
2. Follow the template above
3. Update the relevant slice or feature README with an index link
4. Link dependencies in the execution order document

## Tracking Completion

- Mark work items complete when merged to main
- Update `00-execution-order-and-dependencies.md` to reflect actual vs. planned order
- Archive completed slices to `archive/` if needed

## Related Documentation

- `docs/development-process/branching-and-pr-strategy.md` — How to create feature branches for work items
- `docs/development-process/definition-of-done.md` — What makes a work item "done"
- `docs/development-process/QUICK-REFERENCE.md` — Quick reference for common commands
