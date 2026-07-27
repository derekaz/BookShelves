using BookShelves.Maui.Data.Interfaces;

namespace BookShelves.Maui.Data.Services.Maui;

public sealed class WeatherApiClient : IWeatherApiClient
{
    public HttpClient HttpClient { get; }

    public WeatherApiClient(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }
}
