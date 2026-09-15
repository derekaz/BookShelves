# Slice B Execution Order and Dependencies

This sequence minimizes rework by building the page/navigation shell first, then layering in dialogs, display, and wiring in sequence.

## Recommended Execution Order

1. **WI-013** — Add My Books page and nav entry for authenticated users
   - Creates the page shell and nav entry; downstream WIs depend on this route existing.

2. **WI-014** — Add book action capture dialogs (To-Be-Read, Pages Read, Finished)
   - Dialog components can be developed independently of any page; no page dependency.

3. **WI-015** — Add book action history display on My Books page
   - Populates the My Books page shell with action history list using `IBookUserActionsDataService`.

4. **WI-016** — Wire action buttons into Books index for authenticated users
   - Launches dialogs from WI-014; depends on dialogs existing.

5. **WI-017** — Add Shared UI component and interaction tests
   - Validates WI-013 through WI-016 behavior; depends on all prior WIs being complete.

---

## Dependency Map

- **WI-013**: depends on Slice A WI-007 (web server endpoints for user book actions)
- **WI-014**: depends on WI-013 (route/page shell), Slice A contracts (WI-001, WI-002)
- **WI-015**: depends on WI-013, Slice A WI-006 and WI-007 (service layer and endpoints)
- **WI-016**: depends on WI-014 (dialogs must exist before being launched from index)
- **WI-017**: depends on WI-013, WI-014, WI-015, WI-016

---

## Practical Checkpoints

- **Checkpoint A (navigation + page shell complete):** WI-013 done.
- **Checkpoint B (action capture complete):** WI-014 done; dialogs render and submit correctly.
- **Checkpoint C (full My Books experience complete):** WI-015 done; history visible.
- **Checkpoint D (index integration complete):** WI-016 done; action buttons available from book list.
- **Checkpoint E (verification complete):** WI-017 done.

---

## Notes

- Keep Books/Authors behavior unchanged (global/shared).
- User actions are private to owner; do not render other users' actions.
- Follow existing MudBlazor patterns from `Books/Index.razor` and `BookDetail.razor`.
- All new Blazor components belong in `src/BookShelves.Shared`.
- Tests belong in `test/BookShelves.Shared.Tests`.
