using System.Net;
using System.Net.Http.Json;
using BookShelves.Shared.Data.Models;
using BookShelves.WebApi.Tests.Auth;
using BookShelves.WebApi.Tests.TestUtilities;

namespace BookShelves.WebApi.Tests.Controllers;

public sealed class AuthRestrictionTests(BookShelvesWebApiFactory factory) : IClassFixture<BookShelvesWebApiFactory>
{
    [Fact]
    public async Task Get_TestEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/Test");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_TestEndpoint_WithToken_ReturnsSuccess()
    {
        using var client = factory.CreateClient();
        client.UseTestBearerToken();

        using var response = await client.GetAsync("/Test");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_WeatherForecast_WithTokenButNoScope_ReturnsForbidden()
    {
        using var client = factory.CreateClient();
        client.UseTestBearerToken();

        using var response = await client.GetAsync("/WeatherForecast");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_WeatherForecast_WithWrongScope_ReturnsForbidden()
    {
        using var client = factory.CreateClient();
        client.UseTestBearerToken();
        client.UseTestScopes("Books.Read");

        using var response = await client.GetAsync("/WeatherForecast");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Get_WeatherForecast_WithWeatherScope_ReturnsSuccess()
    {
        using var client = factory.CreateClient();
        client.UseTestBearerToken();
        client.UseTestScopes("Weather.Get");

        using var response = await client.GetAsync("/WeatherForecast");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_WeatherForecast_WithWeatherScope_ReturnsFiveForecastsFromWebApi()
    {
        using var client = factory.CreateClient();
        client.UseTestBearerToken();
        client.UseTestScopes("Weather.Get");

        using var response = await client.GetAsync("/WeatherForecast");
        var payload = await response.Content.ReadFromJsonAsync<List<WeatherForecast>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(payload);
        Assert.Equal(5, payload.Count);
        Assert.All(payload, item => Assert.Equal("Web API", item.Source));
    }
}
