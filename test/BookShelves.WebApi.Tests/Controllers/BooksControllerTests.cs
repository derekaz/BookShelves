using System.Net;
using System.Text;
using System.Text.Json;
using BookShelves.WebApi.Tests.Auth;

namespace BookShelves.WebApi.Tests.Controllers;

public sealed class BooksControllerTests : IClassFixture<BooksControllerWebApiFactory>
{
    private readonly BooksControllerWebApiFactory factory;

    public BooksControllerTests(BooksControllerWebApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Get_Books_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/Books");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_Books_WithTokenButWithoutReadScope_ReturnsForbidden()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        using var response = await client.GetAsync("/Books");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_Books_WithReadScope_ReturnsSuccess()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        client.DefaultRequestHeaders.Add("X-Test-Scopes", "Books.Read");

        using var response = await client.GetAsync("/Books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Books_New_WithReadScopeOnly_ReturnsForbidden()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        client.DefaultRequestHeaders.Add("X-Test-Scopes", "Books.Read");

        using var content = new StringContent(JsonSerializer.Serialize(new
        {
            title = "Test book",
            author = "Test author"
        }), Encoding.UTF8, "application/json");

        using var response = await client.PostAsync("/Books/new", content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_Books_New_WithReadWriteScope_ReturnsCreated()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        client.DefaultRequestHeaders.Add("X-Test-Scopes", "Books.ReadWrite");

        using var content = new StringContent(JsonSerializer.Serialize(new
        {
            title = "Test book",
            author = "Test author"
        }), Encoding.UTF8, "application/json");

        using var response = await client.PostAsync("/Books/new", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
