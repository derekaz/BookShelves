using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddHttpClient<IWeatherForecaster, ClientWeatherForecaster>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

builder.Services.AddScoped<IAuthenticationUIProvider, WasmAuthenticationUIProvider>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFormFactor, ClientFormFactor>();
builder.Services.AddScoped<IVersionService, VersionService>();
builder.Services.AddScoped<IBooksSyncService, BooksSyncService>();

var app = builder.Build();
//app.MapGroup("/authentication").MapLoginAndLogout();
//app.MapGet("/weather-forecast", ([FromServices] IWeatherForecaster WeatherForecaster) =>
//{   
//    return WeatherForecaster.GetWeatherForecastAsync();
//}).RequireAuthorization();  

await app.RunAsync();