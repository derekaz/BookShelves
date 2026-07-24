using BookShelves.WebApi.Tests.Auth;
using System.Net;

namespace BookShelves.WebApi.Tests.Controllers;

public sealed class AuthorItemControllerTests : IClassFixture<AuthorItemWebApiFactory>
{
    private readonly AuthorItemWebApiFactory factory;

    public AuthorItemControllerTests(AuthorItemWebApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Get_AuthorItem_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/tables/Authors");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_AuthorItem_WithToken_ReturnsSuccess()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        using var response = await client.GetAsync("/tables/Authors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
