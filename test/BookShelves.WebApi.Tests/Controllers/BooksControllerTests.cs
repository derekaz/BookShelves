using System.Net;
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

        using var response = await client.GetAsync("/tables/Books");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_Books_WithToken_ReturnsSuccess()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        using var response = await client.GetAsync("/tables/Books");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
