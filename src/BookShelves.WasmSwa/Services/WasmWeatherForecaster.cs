using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;
using System.Net.Http.Json;

namespace BookShelves.WasmSwa.Services;

internal sealed class WasmWeatherForecaster(HttpClient http, ILogger<WasmWeatherForecaster> logger)
    : IWeatherForecaster
{
    private readonly HttpClient _httpClient = http;
    private readonly ILogger _logger = logger;

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        try
        {
            var forecasts = await _httpClient.GetFromJsonAsync<WeatherForecast[]>("/api/weatherforecasts") ?? [];

            foreach (var item in forecasts)
            {
                item.Source = item.Source + " (from WasmWeatherForecastService)";
            }
            return forecasts;
        }
        catch
        {
            return null;
        }
    }
}