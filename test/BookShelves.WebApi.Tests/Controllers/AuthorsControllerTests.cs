using BookShelves.WebApi.Tests.Auth;
using System.Net;

namespace BookShelves.WebApi.Tests.Controllers;

public sealed class AuthorsControllerTests : IClassFixture<AuthorsWebApiFactory>
{
    private readonly AuthorsWebApiFactory factory;

    public AuthorsControllerTests(AuthorsWebApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Get_Authors_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/tables/Authors");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_Authors_WithToken_ReturnsSuccess()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");

        using var response = await client.GetAsync("/tables/Authors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
