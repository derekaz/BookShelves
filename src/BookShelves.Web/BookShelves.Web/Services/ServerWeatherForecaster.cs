using BookShelves.Web.Client.Weather;

namespace BookShelves.Web.Services;

internal sealed class ServerWeatherForecaster(IHttpClientFactory clientFactory) : IWeatherForecaster
{
    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/weatherforecast");
        var client = clientFactory.CreateClient("ExternalApi");
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<WeatherForecast[]>() ??
            throw new IOException("No weather forecast!");
    }
}
