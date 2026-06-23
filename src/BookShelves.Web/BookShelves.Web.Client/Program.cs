using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services.AuthorizationPolicies;
using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Web.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

// Add authorization services - auth state comes from server
builder.Services.AddAuthorizationCore(options =>
{
    options.AddAppAuthorizationPolicies();
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddScoped<IAuthenticationUIProvider, ClientAuthenticationUIProvider>();
builder.Services.AddScoped<IFormFactor, ClientFormFactor>();
builder.Services.AddScoped<IVersionService, ClientVersionService>();
builder.Services.AddScoped<IBooksSyncService, BooksSyncService>();

builder.Services.AddHttpClient<IWeatherForecaster, ClientWeatherForecaster>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

builder.Services.AddScoped<IBookFactory, ClientBookFactory>();

builder.Services.AddHttpClient<IBooksDataService, ClientBooksDataService>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

var app = builder.Build();

await app.RunAsync();