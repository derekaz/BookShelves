using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BookShelves.Web.Shared.Data;

public class WeatherForecastsDataService(HttpClient http, ILogger<BooksDataService> logger) : IWeatherForecaster
{
    private readonly HttpClient _httpClient = http;
    private readonly ILogger _logger = logger;

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<WeatherForecast[]>("/api/weatherforecast") ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "WeatherForecastsDataService:GetWeatherForecastsAsync-Exception");
            return [];
        }
    }

    //public async Task<IEnumerable<IWeatherForecast>> GetWeatherForecastsAsync()
    //{
    //    try
    //    {
    //        return await _httpClient.GetFromJsonAsync<WeatherForecast[]>("/api/weatherforecast") ?? [];
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "WeatherForecastsDataService:GetWeatherForecastsAsync-Exception");
    //        return [];
    //    }
    //}

}
