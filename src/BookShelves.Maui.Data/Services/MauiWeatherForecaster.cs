using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace BookShelves.Maui.Data.Services
{
    public class MauiWeatherForecaster(IHttpClientFactory httpClientFactory, ILogger<MauiWeatherForecaster> logger) : IWeatherForecaster
    {
        //public async Task<IEnumerable<IWeatherForecast>> GetWeatherForecastsAsync()
        //{
        //    // HttpContext.VerifyUserHasAnyAcceptedScope("Weather.Get");
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    });
        //}

        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
        {
            // HttpContext.VerifyUserHasAnyAcceptedScope("Weather.Get");

            logger.LogDebug("Getting weather forecast from API...");
            var forecasts = Array.Empty<WeatherForecast>();

            try
            {
                var httpClient = httpClientFactory.CreateClient("WeatherApi");

                var weatherUrl = "weatherforecast";
                forecasts = (await httpClient.GetFromJsonAsync<WeatherForecast[]>(weatherUrl)) ?? [];
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
}