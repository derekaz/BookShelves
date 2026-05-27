using BookShelves.Shared.Data.Interfaces;
using System.Net.Http.Json;

namespace BookShelves.Web.Client.Weather;

internal sealed class ClientWeatherForecaster(HttpClient httpClient) : IWeatherForecaster
{
    public async Task<IEnumerable<IWeatherForecast>> GetWeatherForecastAsync() =>
        await httpClient.GetFromJsonAsync<IWeatherForecast[]>("/weatherforecast") ??
            throw new IOException("No weather forecast!");
}
