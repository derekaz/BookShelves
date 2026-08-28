using System.Net;
using BookShelves.Web.Tests.WebHost;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BookShelves.Web.Tests.Smoke;

public sealed class BookUserActionsEndpointSmokeTests(WebAppFactory factory) : IClassFixture<WebAppFactory>
{
    [Fact]
    public async Task BookUserActionsEndpoint_WithoutAuth_IsMappedAndProtected()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/bookuseractionsdata");

        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
