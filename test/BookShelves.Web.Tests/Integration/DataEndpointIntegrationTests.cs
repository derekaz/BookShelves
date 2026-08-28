using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Web.Tests.WebHost;

namespace BookShelves.Web.Tests.Integration;

/// <summary>
/// Integration tests for the three data proxy endpoints served by the web host.
/// Tests run against <see cref="AuthenticatedWebAppFactory"/>, which replaces the OIDC
/// auth stack with a test scheme and stubs the downstream data services, so these tests
/// exercise the entire web-host middleware and endpoint pipeline without requiring
/// a running WebApi or real Azure AD tokens.
///
/// Coverage targets:
///   - Unauthenticated requests are rejected (non-200).
///   - Authenticated requests succeed and return the expected JSON shape.
///   - /bookuseractionsdata returns the user's actions (regression for the null-entity
///     access-control 401 and the scope-mismatch MSAL cache-miss bugs).
/// </summary>
public sealed class DataEndpointIntegrationTests(AuthenticatedWebAppFactory factory)
    : IClassFixture<AuthenticatedWebAppFactory>
{
    // -------------------------------------------------------------------------
    // /booksdata
    // -------------------------------------------------------------------------

    [Fact]
    public async Task BooksData_WithoutAuth_ReturnsNonSuccess()
    {
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/booksdata");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task BooksData_WithAuth_ReturnsOkWithBooks()
    {
        using var client = CreateAuthenticatedClient();

        using var response = await client.GetAsync("/booksdata");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array, "Expected a JSON array");
        Assert.True(doc.RootElement.GetArrayLength() > 0, "Expected at least one book");
    }

    // -------------------------------------------------------------------------
    // /authorsdata
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AuthorsData_WithoutAuth_ReturnsNonSuccess()
    {
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/authorsdata");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AuthorsData_WithAuth_ReturnsOkWithAuthors()
    {
        using var client = CreateAuthenticatedClient();

        using var response = await client.GetAsync("/authorsdata");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array, "Expected a JSON array");
        Assert.True(doc.RootElement.GetArrayLength() > 0, "Expected at least one author");
    }

    // -------------------------------------------------------------------------
    // /bookuseractionsdata  — primary regression target
    // -------------------------------------------------------------------------

    [Fact]
    public async Task BookUserActionsData_WithoutAuth_ReturnsNonSuccess()
    {
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/bookuseractionsdata");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Regression: previously the endpoint returned 401/500 for authenticated users
    /// because (a) BearerTokenHandler requested a single scope that did not match the
    /// MSAL cache entry, and (b) BookUserActionsAccessControlProvider.IsAuthorizedAsync
    /// returned false for null-entity Query operations.
    /// </summary>
    [Fact]
    public async Task BookUserActionsData_WithAuth_ReturnsOkWithActions()
    {
        using var client = CreateAuthenticatedClient();

        using var response = await client.GetAsync("/bookuseractionsdata");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.ValueKind == JsonValueKind.Array, "Expected a JSON array");
        Assert.True(doc.RootElement.GetArrayLength() > 0, "Expected at least one book action");
    }

    [Fact]
    public async Task BookUserActionsData_WithAuth_ResponseContainsBookIdAndUserId()
    {
        using var client = CreateAuthenticatedClient();

        using var response = await client.GetAsync("/bookuseractionsdata");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(json);
        var first = doc.RootElement.EnumerateArray().First();
        Assert.True(first.TryGetProperty("bookId", out var bookId) || first.TryGetProperty("BookId", out bookId),
            "Response item should contain a bookId property");
        Assert.False(string.IsNullOrWhiteSpace(bookId.GetString()), "bookId should not be empty");
    }

    [Fact]
    public async Task BookUserActionsData_PostToBeRead_WithValidPayload_ReturnsOk()
    {
        using var client = CreateAuthenticatedClient();

        var now = DateTimeOffset.UtcNow;
        var payload = BookUserActionViewModel.CreateToBeRead(
            bookId: "book-1",
            userId: "web-integration-test-user",
            startTimeUtc: now,
            endTimeUtc: now.AddMinutes(5),
            notes: "queue this");

        using var response = await client.PostAsJsonAsync("/bookuseractionsdata", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task BookUserActionsData_PostToBeRead_MissingEndTime_ReturnsOk()
    {
        using var client = CreateAuthenticatedClient();

        var payload = BookUserActionViewModel.CreateToBeRead(
            bookId: "book-1",
            userId: "web-integration-test-user",
            startTimeUtc: DateTimeOffset.UtcNow,
            endTimeUtc: null,
            notes: "missing end time");

        using var response = await client.PostAsJsonAsync("/bookuseractionsdata", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private HttpClient CreateAuthenticatedClient()
    {
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        return client;
    }
}
