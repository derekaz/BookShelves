using BookShelves.Maui.Data.Interfaces;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BookShelves.Maui.Data.Services.Maui;

public class WeatherForecasterService(IWeatherApiClient httpClient, IHttpClientFactory httpClientFactory, ILogger<WeatherForecasterService> logger) : IWeatherForecasterService
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        // HttpContext.VerifyUserHasAnyAcceptedScope("Weather.Get");

        logger.LogDebug("Getting weather forecast from API...");
        var forecasts = Array.Empty<WeatherForecast>();

        try
        {
            // var httpClient = httpClientFactory.CreateClient("WeatherApi");

            var weatherUrl = "api/weatherforecast";

            forecasts = (await httpClient.HttpClient.GetFromJsonAsync<WeatherForecast[]>(weatherUrl)) ?? [];
        }
        catch (HttpRequestException httpEx)
        {
            logger.LogError(httpEx, "HTTP Request error: {Message}", httpEx.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred: {Message}", ex.Message);
        }

        return forecasts;
    }
}