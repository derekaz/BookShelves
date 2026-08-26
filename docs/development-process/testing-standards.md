# Testing Standards and Best Practices

This document formalizes testing standards and best practices for the BookShelves repository, extending the guidance in `docs/Testing-Strategy.md`.

---

## Overview

Testing in BookShelves follows these principles:

- **Ownership**: Each layer has a dedicated test project (see `Testing-Strategy.md` for ownership map)
- **Quality**: All tests pass before merge; flaky tests are fixed or quarantined
- **Coverage**: Test coverage aligns with risk and layer importance (see `Testing-Strategy.md` for risk map)
- **Clarity**: Tests are readable and follow consistent naming and structure

**Related document**: See `docs/Testing-Strategy.md` for testing baseline, coverage gaps, risk map, and project ownership.

---

## Test Types and When to Use Them

### Unit Tests

**Purpose**: Verify pure logic with no external dependencies (no HTTP, database, or I/O)

**When to use:**
- Business logic (calculations, validations, transformations)
- Shared service logic used by multiple hosts
- Model or DTO behavior

**Example:**
```csharp
[Fact]
public void BookUserAction_WithValidData_CreatesInstance()
{
	// Arrange
	var userId = "user-123";
	var bookId = 1;
	var actionType = UserBookActionType.Read;

	// Act
	var action = new BookUserAction(userId, bookId, actionType);

	// Assert
	Assert.Equal(userId, action.UserId);
	Assert.Equal(bookId, action.BookId);
	Assert.Equal(actionType, action.ActionType);
}
```

**Test Projects:**
- `test/BookShelves.Shared.Tests`
- `test/BookShelves.Web.Shared.Tests`
- `test/BookShelves.Maui.Data.Tests` (for data mapping)

---

### Integration Tests

**Purpose**: Verify behavior across multiple layers (host + dependencies: HTTP, database, auth)

**When to use:**
- Host-level wiring and startup
- Service-to-repository interactions
- Client-to-API communication
- Authorization enforcement

**Example:**
```csharp
[Fact]
public async Task GetUserBooks_WithValidUser_ReturnsUserBooks()
{
	// Arrange
	var factory = new WebApplicationFactory<Program>();
	var client = factory.CreateClient();
	var userId = "user-123";

	// Act
	var response = await client.GetAsync($"/api/books/user/{userId}");

	// Assert
	Assert.True(response.IsSuccessStatusCode);
	var content = await response.Content.ReadAsAsync<List<BookDto>>();
	Assert.NotEmpty(content);
}
```

**Test Projects:**
- `test/BookShelves.WebApi.Tests` (host + API)
- `test/BookShelves.Web.Tests` (host + endpoints)
- `test/BookShelves.Web.Client.Tests` (client + services)

---

### Contract Tests

**Purpose**: Verify API behavior and contracts (status codes, payload shape, error responses)

**When to use:**
- API endpoints (response format, status codes)
- Authorization requirements
- Data contract stability
- Error handling responses

**Example:**
```csharp
[Fact]
public async Task CreateBookUserAction_WithUnauthorizedUser_Returns403Forbidden()
{
	// Arrange
	var factory = new WebApplicationFactory<Program>();
	var client = factory.CreateClient();
	var actionDto = new CreateBookUserActionDto { /* data */ };

	// Act
	var response = await client.PostAsJsonAsync("/api/book-actions", actionDto);

	// Assert
	Assert.Equal(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
}
```

**Test Projects:**
- `test/BookShelves.WebApi.Tests`

---

### UI/Component Smoke Tests

**Purpose**: Minimal coverage of critical user journeys and component rendering

**When to use:**
- Critical user paths (login, main navigation)
- Component rendering with sample data
- Form submission and validation

**Status**: Currently limited in scope (per `copilot-instructions.md`)

**Test Projects:**
- `test/BookShelves.Web.Client.Tests`
- `test/BookShelves.Shared.Tests` (minimal component tests)

---

## Test Naming Convention

Use the pattern: **`MethodName_Condition_ExpectedResult`**

Examples:
```csharp
// Good
GetUserBooks_WithValidUser_ReturnsBooks
CreateBookUserAction_WithUnauthorizedUser_ReturnsForbidden
BookUserAction_WithNullUserId_ThrowsArgumentNullException

// Avoid
Test1
GetUserBooks
UserBooksTest
```

### Rationale

This naming convention makes it immediately clear:
1. **What is being tested** (`MethodName` or feature)
2. **What condition triggers it** (`Condition`)
3. **What the expected outcome is** (`ExpectedResult`)

---

## Arrange-Act-Assert Structure

All tests should follow this pattern:

```csharp
[Fact]
public async Task [TestName]()
{
	// Arrange: Set up test data and mocks
	var userId = "user-123";
	var bookId = 1;
	var mockRepository = new Mock<IUserBookRepository>();
	mockRepository
		.Setup(r => r.GetUserBookAsync(userId, bookId))
		.ReturnsAsync(new UserBook { /* data */ });

	var service = new BookService(mockRepository.Object);

	// Act: Call the method being tested
	var result = await service.GetUserBookAsync(userId, bookId);

	// Assert: Verify the result
	Assert.NotNull(result);
	Assert.Equal(bookId, result.BookId);
	mockRepository.Verify(
		r => r.GetUserBookAsync(userId, bookId),
		Times.Once);
}
```

### Benefits

- **Clear flow**: Easy to understand what the test does
- **Single responsibility**: Each test verifies one behavior
- **Maintainability**: Changes to one section don't affect others
- **Debugging**: Failed assertions point to specific expectations

---

## One Behavioral Assertion per Test

Each test should have **one primary assertion** (plus supporting assertions for completeness).

```csharp
// Good: One behavior being verified
[Fact]
public void GetUserBooks_WithValidUser_ReturnsBooks()
{
	// Arrange
	var service = new BookService(/* mocks */);

	// Act
	var result = await service.GetUserBooksAsync("user-123");

	// Assert: Primary behavior
	Assert.NotEmpty(result);  // Main assertion
	// Supporting assertions for completeness
	Assert.All(result, book => Assert.NotNull(book.Title));
}

// Avoid: Multiple unrelated behaviors
[Fact]
public void GetUserBooks_DoesMultipleThings()
{
	// ... many unrelated assertions ...
	// Hard to debug if one fails
}
```

---

## Test Project Organization

### By Layer

```
test/
  BookShelves.Shared.Tests/
	├── Models/
	│   └── BookUserActionTests.cs
	├── Services/
	│   └── BookServiceTests.cs
	└── Utilities/
		└── TestHelpers.cs

  BookShelves.WebApi.Tests/
	├── Authorization/
	│   └── UserScopeAuthorizationTests.cs
	├── Endpoints/
	│   └── BookUserActionsControllerTests.cs
	└── Fixtures/
		└── ApiWebApplicationFactory.cs
```

### File and Class Naming

- Test file name matches the class being tested: `BookUserAction.cs` → `BookUserActionTests.cs`
- Test class name: `[ClassName]Tests`
- Test method name: `[MethodName]_[Condition]_[ExpectedResult]`

---

## Test Determinism and Reliability

### Rules for Deterministic Tests

1. **No time-based assertions** unless explicitly controlled

   ```csharp
   // Avoid
   Assert.True((DateTime.UtcNow - result.CreatedAt).TotalSeconds < 1);

   // Good: Use time provider or mock clock
   var timeProvider = new FakeTimeProvider(DateTime.UtcNow);
   var result = await service.CreateAsync(data, timeProvider);
   Assert.Equal(timeProvider.UtcNow, result.CreatedAt);
   ```

2. **No random or external data** unless controlled

   ```csharp
   // Avoid
   var data = GetRandomData();  // Non-deterministic

   // Good
   var data = new TestDataBuilder()
	   .WithDefaults()
	   .WithSpecificValue(propertyName, value)
	   .Build();
   ```

3. **No inter-test dependencies**

   ```csharp
   // Avoid: Tests that depend on execution order
   private static int _testCounter = 0;

   [Fact]
   public void Test1() => _testCounter++;

   [Fact]
   public void Test2() => Assert.Equal(1, _testCounter); // Depends on Test1 running first

   // Good: Each test is independent
   [Fact]
   public void Test1() { /* self-contained */ }

   [Fact]
   public void Test2() { /* self-contained */ }
   ```

4. **Isolate external dependencies** with mocks or test doubles

   ```csharp
   // Arrange: Mock external service
   var mockHttpClient = new Mock<HttpClient>();
   mockHttpClient
	   .Setup(c => c.GetAsync(It.IsAny<string>()))
	   .ReturnsAsync(new HttpResponseMessage { StatusCode = 200 });

   // Act & Assert: Now behavior is controlled
   ```

---

## Flaky Test Handling

### Prevention

- Avoid timing-dependent assertions
- Mock external dependencies (network, filesystem)
- Use deterministic test data
- Keep tests small and focused

### Detection

- Run tests multiple times locally before committing
- Monitor `.github/workflows/BookShelves-Flaky-Tests-Monitor.yml` for CI flakiness
- Investigate failures that occur only sometimes

### Resolution

1. **Fix the test** (most common)
   - Remove race conditions
   - Mock non-deterministic dependencies
   - Use explicit waits instead of timeouts

2. **Fix the code** (if test reveals real bug)
   - Synchronization issues
   - Resource cleanup problems
   - Timing-dependent logic

3. **Quarantine** (temporary, if neither is quick)
   - Mark test with `[Trait("Category", "Flaky")]` or `[Ignore("Flaky: GitHub issue #XXX")]`
   - Open an issue to fix properly
   - Don't merge with known flaky tests unless unavoidable

---

## Test Fixtures and Builders

### Reuse Test Helpers

When common setup is repeated, create a helper/builder instead of duplicating code:

```csharp
// Good: Reusable builder
public class BookUserActionBuilder
{
	private string _userId = "default-user";
	private int _bookId = 1;
	private UserBookActionType _actionType = UserBookActionType.Read;

	public BookUserActionBuilder WithUserId(string userId)
	{
		_userId = userId;
		return this;
	}

	public BookUserAction Build()
	{
		return new BookUserAction(_userId, _bookId, _actionType);
	}
}

// Usage in tests
[Fact]
public void SomeTest()
{
	var action = new BookUserActionBuilder()
		.WithUserId("custom-user")
		.Build();

	// Test logic
}
```

### Test Factories

For integration tests, use a factory to create configured hosts:

```csharp
// Good: WebApplicationFactory in test project
public class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureServices(services =>
		{
			// Replace real dependencies with test doubles
			services.AddScoped(_ => new Mock<IUserRepository>().Object);
		});
	}
}

// Usage
var factory = new ApiWebApplicationFactory();
var client = factory.CreateClient();
var response = await client.GetAsync("/api/endpoint");
```

---

## Mocking and Test Doubles

### When to Mock

- **External services** (HTTP, database queries outside the test scope)
- **Non-deterministic behavior** (time, random, file system)
- **Hard-to-reproduce errors** (network timeouts, auth failures)

### When NOT to Mock

- **Code you're testing** — test the real implementation
- **Simple value objects** — no need to mock
- **Integration points** — test real interactions at boundaries

### Best Practices

```csharp
// Good: Mock external dependency, test real business logic
var mockUserRepository = new Mock<IUserRepository>();
mockUserRepository
	.Setup(r => r.GetUserAsync("user-123"))
	.ReturnsAsync(new User { Id = "user-123", Name = "Test User" });

var bookService = new BookService(mockUserRepository.Object);

var result = await bookService.GetUserBooksAsync("user-123");

// Bad: Mocking too much (what are we testing?)
var mockEverything = new Mock<IEverything>();
// ...can't tell if test is testing the code or the mocks
```

---

## Test Coverage and Ownership

See `docs/Testing-Strategy.md` for:
- Test project ownership map
- Coverage gap inventory
- Risk-based prioritization

Key principle: **Coverage aligns with risk, not line count.**

---

## Code Review Checklist for Tests

Before approving a PR with tests:

- [ ] Test method name is clear and follows `MethodName_Condition_ExpectedResult`
- [ ] Test uses Arrange-Act-Assert structure
- [ ] Test has one primary behavior being verified
- [ ] Test is deterministic (no timing, randomness, or order dependencies)
- [ ] Mocks are appropriate (not mocking the code being tested)
- [ ] Test would fail if the implementation was removed
- [ ] Test is in the correct test project (per ownership map)
- [ ] No duplicate setup across multiple tests (refactor to builder/factory)
- [ ] Assertions are specific (not just `Assert.NotNull`)
- [ ] Test runs quickly (sub-second for unit tests)

---

## Running Tests Locally

```powershell
# Run all tests
dotnet test "BookShelves (Maui, Web and WebApi).slnx"

# Run specific test project
dotnet test test/BookShelves.WebApi.Tests/BookShelves.WebApi.Tests.csproj

# Run specific test class
dotnet test test/BookShelves.WebApi.Tests/BookShelves.WebApi.Tests.csproj --filter "ClassName=BookUserActionsControllerTests"

# Run specific test method
dotnet test test/BookShelves.WebApi.Tests/BookShelves.WebApi.Tests.csproj --filter "FullyQualifiedName=BookShelves.WebApi.Tests.BookUserActionsControllerTests.GetUserBooks_WithValidUser_ReturnsBooks"

# Run with verbose output
dotnet test "BookShelves (Maui, Web and WebApi).slnx" --verbosity detailed

# Generate coverage report
dotnet test "BookShelves (Maui, Web and WebApi).slnx" /p:CollectCoverage=true
```

---

## Continuous Integration

All tests run automatically on:

- Every push to `main`
- Every pull request
- Nightly scheduled runs (flaky test monitoring)

See `.github/workflows/` for pipeline definitions.

---

## Related Documentation

- `docs/Testing-Strategy.md` — Strategic test planning and ownership
- `definition-of-done.md` — Quality gates for features and bug fixes
- `code-standards-and-conventions.md` — Code quality expectations

