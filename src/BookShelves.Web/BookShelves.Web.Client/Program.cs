using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services;
using BookShelves.Shared.Services.AuthorizationPolicies;
using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Web.Client.Handlers;
using BookShelves.Web.Client.Services.Client;
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

builder.Services.AddScoped<IAuthenticationUIProvider, AuthenticationUIProviderService>();
builder.Services.AddScoped<IFormFactor, FormFactorService>();
builder.Services.AddScoped<IVersionService, VersionService>();
builder.Services.AddScoped<IDocumentsFolderAccessService, DocumentsFolderAccessService>();

builder.Services.AddScoped<ISyncDataService, SyncDataService>();
builder.Services.AddScoped<ISyncProgressService, SyncProgressService>();
builder.Services.AddScoped<IPageSyncCoordinator, PageSyncCoordinator>();

builder.Services.AddHttpClient<IWeatherForecasterService, WeatherForecasterService>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

builder.Services.AddTransient<BlazorAuthorizationHandler>();

builder.Services.AddHttpClient<IBooksDataService, BooksDataService>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
}).AddHttpMessageHandler<BlazorAuthorizationHandler>();

builder.Services.AddHttpClient<IAuthorsDataService, AuthorsDataService>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
}).AddHttpMessageHandler<BlazorAuthorizationHandler>();

builder.Services.AddHttpClient<IBookUserActionsDataService, BookUserActionsDataService>(httpClient =>
{
    httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
}).AddHttpMessageHandler<BlazorAuthorizationHandler>();

var app = builder.Build();

await app.RunAsync();