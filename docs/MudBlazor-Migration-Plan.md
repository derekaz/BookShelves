# MudBlazor Migration and Bootstrap Removal Plan for BookShelves

This document replaces the earlier draft with a more complete plan for moving the shared UI to MudBlazor-first styling and removing Bootstrap from the app’s shared experience.

## Objective

Move BookShelves to a single, consistent UI system based on MudBlazor and remove Bootstrap from the shared UI path so the MAUI app and the web app both feel like the same product.

## Scope

This plan focuses on the shared UI in `src/BookShelves.Shared` and the host entry points that still pull in Bootstrap assets. It does not require a rewrite of the app’s domain logic, services, or data model.

## Current assessment

The shared layout and navigation appear to be largely complete from a MudBlazor standpoint:

- `src/BookShelves.Shared/Components/Layout/MainLayout.razor` already uses MudBlazor providers, `MudLayout`, `MudAppBar`, `MudDrawer`, and `MudMainContent`.
- `src/BookShelves.Shared/Components/Layout/NavMenu.razor` already uses `MudNavMenu`, `MudNavLink`, `MudStack`, `MudPaper`, and `MudButton` for the shell and footer actions.
- The remaining work is now mostly in page-level content, not in the shell itself.

That means the plan should shift from “replace layout/navigation” to “complete the content-page migration and remove the remaining Bootstrap dependency from the app surface.”

## Current state to address

The repository already uses MudBlazor in several shared layout components, but there is still a mix of Bootstrap markup and Bootstrap-style classes in the shared pages. Examples include:

- Bootstrap CSS being loaded from `src/BookShelves.Web/BookShelves.Web/Components/WebApp.razor`
- Bootstrap button classes such as `btn btn-primary` and `btn btn-secondary`
- Bootstrap table markup in pages such as `Books/Index.razor`, `Authors/Index.razor`, `UserProfile.razor`, and `Weather.razor`
- Bootstrap alert markup in pages such as `Admin.razor`

The migration should therefore be treated as a component-by-component replacement rather than a one-shot redesign.

## Guiding principles

- Preserve behavior first; styling second.
- Migrate one page or feature at a time.
- Prefer replacing a small Bootstrap block with the closest MudBlazor equivalent over a broad rewrite.
- Keep shared logic and services unchanged unless the UI migration requires it.
- Make the MAUI and web hosts converge on the same visual system.
- Do not introduce new Bootstrap usage in new pages or features.

## Goals

- Make the shared UI visually consistent across MAUI and Web.
- Eliminate Bootstrap as the default styling system for shared pages.
- Reduce styling drift caused by mixing Bootstrap and MudBlazor.
- Lower long-term maintenance cost by relying on one component system.
- Improve responsiveness and mobile friendliness for the MAUI experience.

## Non-goals

- No full rewrite of the application architecture.
- No redesign of every page purely for aesthetics.
- No changes to business logic unless required to preserve behavior during UI migration.

## Phase 1: Audit and inventory the current usage

Focus: identify the concrete places where Bootstrap is still in use and define the migration order.

### Work items
- Inventory all shared Razor pages that still use Bootstrap classes or Bootstrap-only markup.
- Identify the host-level stylesheet references that still load Bootstrap.
- List pages that are high-value targets first: shell, navigation, CRUD pages, list pages, demo/test pages.
- Create a simple migration checklist and keep it updated as pages are migrated.

### Primary files to review
- `src/BookShelves.Shared/Components/Layout/MainLayout.razor`
- `src/BookShelves.Shared/Components/Layout/NavMenu.razor`
- `src/BookShelves.Shared/Components/Pages/Home.razor`
- `src/BookShelves.Shared/Components/Pages/Counter.razor`
- `src/BookShelves.Shared/Components/Pages/Admin.razor`
- `src/BookShelves.Shared/Components/Pages/Weather.razor`
- `src/BookShelves.Shared/Components/Pages/UserProfile.razor`
- `src/BookShelves.Shared/Components/Pages/Books/Index.razor`
- `src/BookShelves.Shared/Components/Pages/Books/BookDetail.razor`
- `src/BookShelves.Shared/Components/Pages/Books/BookDelete.razor`
- `src/BookShelves.Shared/Components/Pages/Authors/Index.razor`
- `src/BookShelves.Shared/Components/Pages/Authors/AuthorDetail.razor`
- `src/BookShelves.Shared/Components/Pages/Authors/AuthorDelete.razor`
- `src/BookShelves.Web/BookShelves.Web/Components/WebApp.razor`

### Outcome
A clear list of pages, components, and host assets that must be migrated before Bootstrap can be removed safely.

## Phase 2: Stabilize the MudBlazor shell and shared theme

Focus: establish the shared layout foundation before migrating individual pages.

### Work items
- Keep and refine MudBlazor providers in `src/BookShelves.Shared/Components/Layout/MainLayout.razor`.
- Define a shared BookShelves theme palette using MudBlazor theme settings.
- Standardize spacing, typography, and action styling around MudBlazor defaults.
- Keep the migration limited to the shared UI so both hosts benefit immediately.
- Replace ad-hoc layout wrappers with MudBlazor layout components where appropriate.

### Primary files
- `src/BookShelves.Shared/Components/Layout/MainLayout.razor`
- `src/BookShelves.Shared/Components/Layout/NavMenu.razor`
- `src/BookShelves.Shared/wwwroot/css/app.css`

### Outcome
A stable, themable shell that looks consistent before deeper page-level migration begins.

## Phase 3: Shift from shell work to content pages and common controls

Focus: the remaining shared UI surface now that the shell and navigation are effectively in place.

### Work items
- Treat the shell and navigation as mostly complete and focus on page-level migration instead of further structural layout work.
- Replace Bootstrap buttons, alerts, and action rows with MudBlazor components such as `MudButton`, `MudIconButton`, `MudAlert`, `MudStack`, `MudPaper`, and `MudGrid`.
- Keep the existing routing logic, authorization checks, event handlers, and data binding intact.
- Prioritize the pages that still show Bootstrap usage in their markup and interactions.

### Primary files
- `src/BookShelves.Shared/Components/Pages/Home.razor`
- `src/BookShelves.Shared/Components/Pages/Counter.razor`
- `src/BookShelves.Shared/Components/Pages/Admin.razor`
- `src/BookShelves.Shared/Components/Pages/Books/BookDelete.razor`
- `src/BookShelves.Shared/Components/Pages/Authors/AuthorDelete.razor`

### Outcome
The migration moves from layout completion into content and interaction refinement.

## Phase 4: Replace common interactive controls and simple pages

Focus: buttons, alerts, and simple page-level actions.

### Work items
- Replace Bootstrap buttons with `MudButton` and `MudIconButton`.
- Replace Bootstrap alerts with `MudAlert`.
- Replace simple page action rows with `MudStack`, `MudPaper`, or `MudGrid`.
- Keep the existing event handlers and data binding intact.
- Migrate lower-risk pages first so visual progress is visible quickly.

### Priority pages
- `src/BookShelves.Shared/Components/Pages/Home.razor`
- `src/BookShelves.Shared/Components/Pages/Counter.razor`
- `src/BookShelves.Shared/Components/Pages/Admin.razor`
- `src/BookShelves.Shared/Components/Pages/Books/BookDelete.razor`
- `src/BookShelves.Shared/Components/Pages/Authors/AuthorDelete.razor`

### Outcome
The app starts to feel like a single system instead of a mix of Bootstrap and MudBlazor.

## Phase 5: Migrate forms and dialogs

Focus: the most important interactive workflows.

### Work items
- Replace Bootstrap form layouts with MudBlazor form components such as `MudTextField`, `MudTextArea`, `MudSelect`, `MudDatePicker`, and `MudForm`.
- Keep existing validation flow and data binding intact.
- Migrate dialogs to MudBlazor dialog patterns using `MudDialog` or the existing shared dialog infrastructure.
- Introduce a consistent shared form layout pattern for CRUD pages.

### Priority files
- `src/BookShelves.Shared/Components/Pages/Books/BookDetail.razor`
- `src/BookShelves.Shared/Components/Pages/Authors/AuthorDetail.razor`
- `src/BookShelves.Shared/Components/Pages/ModalTest1/CustomModal.razor`
- `src/BookShelves.Shared/Components/Pages/ModalTest2/CustomDialog.razor`

### Outcome
The pages with the most interaction and user input become consistent and easier to maintain.

## Phase 6: Replace list and table views

Focus: data-heavy pages.

### Work items
- Replace Bootstrap table markup with `MudTable` or `MudDataGrid`.
- Preserve existing sorting, filtering, and action columns where they exist.
- Keep the existing service calls and view models unchanged.
- Move list-view layout into MudBlazor-friendly container structures.

### Priority files
- `src/BookShelves.Shared/Components/Pages/Books/Index.razor`
- `src/BookShelves.Shared/Components/Pages/Authors/Index.razor`
- `src/BookShelves.Shared/Components/Pages/UserProfile.razor`
- `src/BookShelves.Shared/Components/Pages/Weather.razor`

### Outcome
The highest-traffic list views adopt MudBlazor patterns and no longer depend on table classes from Bootstrap.

## Phase 7: Remove Bootstrap from the app shell and shared assets

Focus: remove the dependency rather than just hiding it.

### Work items
- Remove the Bootstrap stylesheet link from the web host entry point in `src/BookShelves.Web/BookShelves.Web/Components/WebApp.razor`.
- Remove any shared usage of Bootstrap-only CSS classes from Razor components.
- Replace or delete any app CSS rules that exist only to support Bootstrap-specific markup.
- Keep only the minimal global CSS that is still needed for non-component concerns.
- Ensure the shared UI builds and renders correctly once Bootstrap is gone.

### Outcome
The shared UI is no longer coupled to Bootstrap and can be maintained using a single component system.

## Phase 8: Final cleanup and guardrails

Focus: keep the project from slipping back into a Bootstrap mix.

### Work items
- Review the remaining pages and decide whether demo/test pages should be migrated or removed.
- Add a simple contribution rule that new shared pages should use MudBlazor by default.
- Update any relevant documentation if the UI architecture changes.
- Re-run build and UI validation after each wave of migration rather than waiting until the end.

### Outcome
The codebase stays aligned with the MudBlazor-first approach over time.

## Recommended implementation order

1. Audit and inventory existing Bootstrap usage. This is largely complete, but should be kept as a checklist while migrating pages.
2. Maintain and polish the MudBlazor shell and theme. The core shell is already in place.
3. Migrate simple pages and common controls.
4. Migrate forms and dialogs.
5. Migrate list and table views.
6. Remove Bootstrap asset references and clean up deprecated styling from the shared UI and host entry points.
7. Add guardrails so new work stays MudBlazor-first.

## Suggested first wave for low risk and high visibility

Start with these files because they give the biggest visual payoff with the least disruption:

- `src/BookShelves.Shared/Components/Layout/MainLayout.razor`
- `src/BookShelves.Shared/Components/Layout/NavMenu.razor`
- `src/BookShelves.Shared/Components/Pages/Home.razor`
- `src/BookShelves.Shared/Components/Pages/Books/Index.razor`
- `src/BookShelves.Shared/Components/Pages/Books/BookDetail.razor`
- `src/BookShelves.Shared/Components/Pages/Books/BookDelete.razor`

## Definition of done

The migration can be considered complete when all of the following are true:

- MudBlazor is the default component system for the shared UI.
- Shared Razor pages no longer depend on Bootstrap classes for core layout or interaction.
- The web host no longer loads Bootstrap as part of the shared UI path.
- The MAUI and web experiences feel visually consistent.
- New pages are built with MudBlazor by default.
