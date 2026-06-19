using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;
using System.Net.Http.Json;

namespace BookShelves.WasmSwa.Services;

internal sealed class WasmWeatherForecaster(HttpClient http, ILogger<WasmWeatherForecaster> logger)
    : IWeatherForecaster
{
    private readonly HttpClient _httpClient = http;
    private readonly ILogger _logger = logger;

    //public WasmWeatherForecaster(//HttpClient? httpClient, 
    //    IDownstreamApi downstreamApi, IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authenticationStateProvider)
    //{
    //    //_httpClient = httpClient;
    //    _downstreamApi = downstreamApi;
    //    _contextAccessor = httpContextAccessor;
    //    _authenticationStateProvider = authenticationStateProvider;
    //}

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        try
        {
            var forecasts = await _httpClient.GetFromJsonAsync<WeatherForecast[]>("/api/weatherforecasts") ?? [];

            //var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            //var curUser = authState.User;
            //var loginHint = user.GetObjectId(); // or user.GetUpn()

            //using var request = new HttpRequestMessage(HttpMethod.Get, "/weatherforecast");

            //var client = 
            //using var response = await _downstreamApi.CallApiForUserAsync("WeatherApi",
            //    options =>
            //    {
            //        options.RelativePath = "/weatherforecast";
            //    }, curUser);

            //response.EnsureSuccessStatusCode();
            //var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>() ??
            //    throw new IOException("No weather forecast!");

            foreach (var item in forecasts)
            {
                item.Source = item.Source + " (from WasmWeatherForecastService)";
            }
            return forecasts;

            // return await _downstreamApi.GetForUserAsync<WeatherForecast[]>("WeatherApi");
            //return await _httpClient.GetFromJsonAsync<WeatherForecast[]>("weatherforecast");
        }
        catch
        {
            return null;
        }





        //using var request = new HttpRequestMessage(HttpMethod.Get, "/weatherforecast");
        //var client = clientFactory.CreateClient("ExternalApi");
        //using var response = await client.SendAsync(request);
        //response.EnsureSuccessStatusCode();

        //var temp = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        //var result = temp ?? throw new IOException("No weather forecast!"); //.Cast<IWeatherForecast>();
        //return result ?? throw new IOException("No weather forecast!");
    }
}