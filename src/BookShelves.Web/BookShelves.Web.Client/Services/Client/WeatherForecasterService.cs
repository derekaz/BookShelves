using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;
using System.Net.Http.Json;

namespace BookShelves.Web.Client.Services.Client;

internal sealed class WeatherForecasterService(HttpClient httpClient) : IWeatherForecasterService
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        var temp = await httpClient.GetFromJsonAsync<WeatherForecast[]>("/weatherforecast");

        if (temp != null)
        {
            foreach (var forecast in temp)
            {
                forecast.Source = "(via ClientWeatherForecaster) " + forecast.Source;
            }
        }

        var result = temp ?? throw new IOException("No weather forecast!");

        return result ?? throw new IOException("No weather forecast!");
    }
}
