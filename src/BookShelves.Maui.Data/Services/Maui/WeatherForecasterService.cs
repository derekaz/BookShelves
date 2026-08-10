using BookShelves.Maui.Data.Interfaces;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BookShelves.Maui.Data.Services.Maui;

public class WeatherForecasterService(IWeatherApiClient httpClient, ILogger<WeatherForecasterService> logger) : IWeatherForecasterService
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        // HttpContext.VerifyUserHasAnyAcceptedScope("Weather.Get");

        logger.LogDebug("Getting weather forecast from API...");
        var forecasts = Array.Empty<WeatherForecast>();
        Uri? requestUri = null;
        string? responseBody = null;

        try
        {
            var weatherUrl = "weatherforecast";
            requestUri = httpClient.HttpClient.BaseAddress is not null
                ? new Uri(httpClient.HttpClient.BaseAddress, weatherUrl)
                : new Uri(weatherUrl, UriKind.RelativeOrAbsolute);

            logger.LogDebug("Calling weather API at {RequestUri}", requestUri);

            using var response = await httpClient.HttpClient.GetAsync(weatherUrl, HttpCompletionOption.ResponseHeadersRead);
            responseBody = await response.Content.ReadAsStringAsync();

            logger.LogDebug("Weather API response status {StatusCode} for {RequestUri}", (int)response.StatusCode, requestUri);
            logger.LogDebug("Weather API response body: {ResponseBody}", responseBody);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Weather API request failed with status {StatusCode} for {RequestUri}. Response body: {ResponseBody}", (int)response.StatusCode, requestUri, responseBody);
                return [];
            }

            forecasts = JsonSerializer.Deserialize<WeatherForecast[]>(responseBody, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? [];
        }
        catch (HttpRequestException httpEx)
        {
            logger.LogError(httpEx, "HTTP Request error for weather API. RequestUri: {RequestUri}. Message: {Message}", requestUri, httpEx.Message);
        }
        catch (JsonException jsonEx)
        {
            logger.LogError(jsonEx, "Weather API returned invalid JSON for {RequestUri}. Response body: {ResponseBody}", requestUri, responseBody);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred: {Message}", ex.Message);
        }

        return forecasts;
    }
}