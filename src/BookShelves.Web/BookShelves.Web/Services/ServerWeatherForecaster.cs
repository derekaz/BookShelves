using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Abstractions;

namespace BookShelves.Web.Services;

internal sealed class ServerWeatherForecaster  //IHttpClientFactory clientFactory)
    : IWeatherForecaster
{
    private readonly IDownstreamApi _downstreamApi;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public ServerWeatherForecaster(//HttpClient? httpClient, 
        IDownstreamApi downstreamApi, IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authenticationStateProvider)
    {
        //_httpClient = httpClient;
        _downstreamApi = downstreamApi;
        _contextAccessor = httpContextAccessor;
        _authenticationStateProvider = authenticationStateProvider;
    }

    public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
    {
        try
        {
            HttpContext? context = _contextAccessor.HttpContext;
            var curUser = context?.User;

            //var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            //var curUser = authState.User;
            //var loginHint = user.GetObjectId(); // or user.GetUpn()

            //using var request = new HttpRequestMessage(HttpMethod.Get, "/weatherforecast");

            //var client = 
            using var response = await _downstreamApi.CallApiForUserAsync("WeatherApi",
                options =>
                {
                    options.RelativePath = "/weatherforecast";
                }, curUser);

            response.EnsureSuccessStatusCode();
            var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>() ??
                throw new IOException("No weather forecast!");

            foreach (var forecast in forecasts)
            {
                forecast.Source = "(via ServerWeatherForecaster) " + forecast.Source;
            }
            return forecasts;

            // return await _downstreamApi.GetForUserAsync<WeatherForecast[]>("WeatherApi");
            //return await _httpClient.GetFromJsonAsync<WeatherForecast[]>("weatherforecast");
        }
        catch
        {
            return [];
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