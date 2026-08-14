using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using BookShelves.Shared.Data.Models;
using BookShelves.Web.Services.Server;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Abstractions;
using Moq;

namespace BookShelves.Web.Tests.Services;

public sealed class WeatherForecasterServiceTests
{
    [Fact]
    public async Task GetWeatherForecastAsync_WhenDownstreamReturnsForecasts_PrefixesSource()
    {
        Action<DownstreamApiOptions>? capturedOptions = null;
        var downstreamApi = new Mock<IDownstreamApi>(MockBehavior.Strict);
        downstreamApi
            .Setup(x => x.CallApiForUserAsync(
                "WeatherApi",
                It.IsAny<Action<DownstreamApiOptions>>(),
                It.IsAny<ClaimsPrincipal?>(),
                It.IsAny<HttpContent?>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, Action<DownstreamApiOptions>, ClaimsPrincipal?, HttpContent?, CancellationToken>((_, options, _, _, _) => capturedOptions = options)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new[]
                {
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.UtcNow),
                        TemperatureC = 12,
                        Summary = "Cool",
                        Source = "Web API"
                    }
                })
            });

        var contextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "tester")], "test"))
            }
        };

        var sut = new WeatherForecasterService(downstreamApi.Object, contextAccessor, new StubAuthenticationStateProvider());

        var result = (await sut.GetWeatherForecastAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("(via ServerWeatherForecaster) Web API", result[0].Source);

        Assert.NotNull(capturedOptions);
        var options = new DownstreamApiOptions();
        capturedOptions!(options);
        Assert.Equal("/weatherforecast", options.RelativePath);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_WhenDownstreamThrows_ReturnsEmpty()
    {
        var downstreamApi = new Mock<IDownstreamApi>(MockBehavior.Strict);
        downstreamApi
            .Setup(x => x.CallApiForUserAsync(
                "WeatherApi",
                It.IsAny<Action<DownstreamApiOptions>>(),
                It.IsAny<ClaimsPrincipal?>(),
                It.IsAny<HttpContent?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var contextAccessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var sut = new WeatherForecasterService(downstreamApi.Object, contextAccessor, new StubAuthenticationStateProvider());

        var result = (await sut.GetWeatherForecastAsync()).ToList();

        Assert.Empty(result);
    }

    private sealed class StubAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
    }
}
