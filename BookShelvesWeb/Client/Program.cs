using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorApp.Client;
using AzureStaticWebApps.Blazor.Authentication;
using Blazored.Modal;
using BlazorApp.Client.Data;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped(sp => 
    new HttpClient 
        { BaseAddress = new Uri(builder.Configuration["API_Prefix"] ?? builder.HostEnvironment.BaseAddress) }
    );

builder.Services.AddScoped<IBooksDataService, BooksDataService>();

builder.Services.AddStaticWebAppsAuthentication();
builder.Services.AddBlazoredModal();

await builder.Build().RunAsync();
