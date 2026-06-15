using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;

namespace BookShelves.Web.Services;

internal sealed class ServerWeatherForecaster(IHttpClientFactory clientFactory) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/weatherforecast");
        var client = clientFactory.CreateClient("ExternalApi");
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var temp = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        var result = temp ?? throw new IOException("No weather forecast!"); //.Cast<IWeatherForecast>();
        return result ?? throw new IOException("No weather forecast!");
    }
}