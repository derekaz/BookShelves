using System.Net;
using BookShelves.Maui.Data.Interfaces;
using BookShelves.Maui.Data.Services.Maui;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookShelves.Maui.Data.Tests.Services;

public sealed class WeatherForecasterServiceTests
{
    [Fact]
    public async Task GetWeatherForecastAsync_ReturnsForecasts_WhenApiReturnsValidJson()
    {
        var json = "[{\"date\":\"2026-08-13\",\"temperatureC\":25,\"summary\":\"Warm\",\"source\":\"API\"}]";
        var apiClient = CreateWeatherApiClient(HttpStatusCode.OK, json);
        var logger = new Mock<ILogger<WeatherForecasterService>>();
        var sut = new WeatherForecasterService(apiClient.Object, logger.Object);

        var result = (await sut.GetWeatherForecastAsync()).ToList();

        Assert.Single(result);
        Assert.Equal(25, result[0].TemperatureC);
        Assert.Equal("Warm", result[0].Summary);
        Assert.Equal("API", result[0].Source);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_ReturnsEmpty_WhenStatusCodeIsNotSuccessful()
    {
        var apiClient = CreateWeatherApiClient(HttpStatusCode.InternalServerError, "{\"error\":\"failure\"}");
        var logger = new Mock<ILogger<WeatherForecasterService>>();
        var sut = new WeatherForecasterService(apiClient.Object, logger.Object);

        var result = await sut.GetWeatherForecastAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_ReturnsEmpty_WhenJsonIsInvalid()
    {
        var apiClient = CreateWeatherApiClient(HttpStatusCode.OK, "not-json");
        var logger = new Mock<ILogger<WeatherForecasterService>>();
        var sut = new WeatherForecasterService(apiClient.Object, logger.Object);

        var result = await sut.GetWeatherForecastAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_ReturnsEmpty_WhenHttpRequestThrows()
    {
        var apiClient = CreateThrowingWeatherApiClient(() => new HttpRequestException("network"));
        var logger = new Mock<ILogger<WeatherForecasterService>>();
        var sut = new WeatherForecasterService(apiClient.Object, logger.Object);

        var result = await sut.GetWeatherForecastAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWeatherForecastAsync_ReturnsEmpty_WhenUnexpectedExceptionOccurs()
    {
        var apiClient = CreateThrowingWeatherApiClient(() => new InvalidOperationException("boom"));
        var logger = new Mock<ILogger<WeatherForecasterService>>();
        var sut = new WeatherForecasterService(apiClient.Object, logger.Object);

        var result = await sut.GetWeatherForecastAsync();

        Assert.Empty(result);
    }

    private static Mock<IWeatherApiClient> CreateWeatherApiClient(HttpStatusCode statusCode, string content)
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content)
        });

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var apiClient = new Mock<IWeatherApiClient>();
        apiClient.Setup(x => x.HttpClient).Returns(client);
        return apiClient;
    }

    private static Mock<IWeatherApiClient> CreateThrowingWeatherApiClient(Func<Exception> exceptionFactory)
    {
        var handler = new StubThrowingHttpMessageHandler(_ => throw exceptionFactory());

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var apiClient = new Mock<IWeatherApiClient>();
        apiClient.Setup(x => x.HttpClient).Returns(client);
        return apiClient;
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }

    private sealed class StubThrowingHttpMessageHandler(Func<HttpRequestMessage, Exception> thrower) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromException<HttpResponseMessage>(thrower(request));
    }
}
