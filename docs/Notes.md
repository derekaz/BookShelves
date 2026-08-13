# Notes and Historical References

This file is a scratchpad for ideas and reference links that are not yet formalized elsewhere.
Treat it as non-authoritative. If a note becomes important, move it into a purpose-built doc under `docs/`.

## Current References

- Microsoft Blazor samples: https://github.com/dotnet/blazor-samples
- CommunityToolkit Datasync: https://github.com/CommunityToolkit/Datasync

## What Moved Out

- Docker and reverse-proxy notes: `docs/Docker-and-Networking.md`
- Versioning and release notes: `docs/Versioning-and-Release.md`

## See Also

- `docs/Developer-and-AI-Guidance.md`
- `docs/Solution-Structure.md`
- `docs/Build-Test-Run.md`

## Open Questions

- Whether any remaining scratchpad content should be promoted into formal guidance


## Chat about testing...
### Good starting point is a layered test strategy, beginning with the easiest/high-value units first.
1. BookShelves.Shared (start here first)
   - Best ROI for pure unit tests.
   - Focus on:
		- service classes with business rules
		- authorization policy handlers
		- model validation/transformation logic
   - Use standard xUnit + mocking (Moq/NSubstitute) with no MAUI host required.
1. BookShelves.Maui.Data (next)
   - Add tests around:
      - repository behavior
	  - sync orchestration logic
      - conflict handling / mapping
   - Prefer SQLite in-memory or temp-file DB tests for EF Core behavior.
	  - Split into:
		 - unit tests for service logic
		 - integration tests for DbContext/repository queries
1. BookShelves.Maui (after that)
  - Start with non-UI services first:
	 - AuthenticationService flows (silent token, fallback interactive, sign-out state reset)
	    - ExternalAuthenticationStateProvider state transitions
		- MauiAuthenticationMessageHandler header behavior and exception path behavior
     - Keep these as unit tests with mocked dependencies.
     - UI/Blazor component tests can come later (bUnit), and platform-specific auth partials are better treated as smoke/integration tests.

#### Recommended test project layout
	test/BookShelves.Shared.Tests
	test/BookShelves.Maui.Data.Tests
	test/BookShelves.Maui.Tests

#### Order to implement
1. Shared unit tests
1. Maui.Data unit + EF integration tests
1. Maui service-level auth tests

If needed, a next step would be to define the first 10 concrete test cases by class/method for those three projects.