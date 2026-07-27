using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Abstractions;

namespace BookShelves.Web.Services.Server;

internal sealed class WeatherForecasterService
    : IWeatherForecasterService
{
    private readonly IDownstreamApi _downstreamApi;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly AuthenticationStateProvider _authenticationStateProvider;

    public WeatherForecasterService(
        IDownstreamApi downstreamApi, IHttpContextAccessor httpContextAccessor, AuthenticationStateProvider authenticationStateProvider)
    {
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
        }
        catch
        {
            return [];
        }
    }
}