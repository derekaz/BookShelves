using System.Net;
using BookShelves.Web.Tests.WebHost;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BookShelves.Web.Tests.Smoke;

public sealed class WebHostSmokeTests(WebAppFactory factory) : IClassFixture<WebAppFactory>
{
    [Fact]
    public async Task RootEndpoint_ReturnsSuccessOrRedirect()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/");

        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Redirect ||
            response.StatusCode == HttpStatusCode.TemporaryRedirect,
            $"Unexpected status code: {response.StatusCode}");
    }

    [Fact]
    public async Task UnknownEndpoint_ReturnsNotFound()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/smoke/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task WeatherForecastEndpoint_WithoutAuth_DoesNotReturnSuccess()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/weatherforecast");

        Assert.True(
            response.StatusCode != HttpStatusCode.OK,
            "Expected non-success response for unauthenticated weather endpoint request.");
    }
}
