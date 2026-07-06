using BookShelves.Shared;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services.AuthorizationPolicies;
using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.WasmSwa;
using BookShelves.WasmSwa.Data;
using BookShelves.WasmSwa.Services;
using BookShelves.Web.Shared.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<WasmApp>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRazorClassLibraryServices();

builder.Services.AddMudServices();

builder.Services.AddSingleton<IFormFactor, WasmFormFactor>();
builder.Services.AddSingleton<IVersionService, VersionService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddTransient<IBook, Book>();
builder.Services.AddTransient<IBookFactory, BookViewModelFactory>();
builder.Services.AddTransient<IBooksDataService, BooksDataService>();
builder.Services.AddTransient<IBooksSyncService, BooksSyncService>();
builder.Services.AddTransient<IWeatherForecaster, WasmWeatherForecaster>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuthenticationUIProvider, WasmAuthenticationUIProvider>();
builder.Services.AddScoped<IGraphService, GraphService>();

// Add authorization services - auth state comes from server
builder.Services.AddAuthorizationCore(options =>
{
    options.AddAppAuthorizationPolicies();
});
builder.Services.AddCascadingAuthenticationState();

var baseUrl = string.Join("/",
    builder.Configuration.GetSection("MicrosoftGraph")["BaseUrl"],
    builder.Configuration.GetSection("MicrosoftGraph")["Version"]);
var scopes = builder.Configuration.GetSection("MicrosoftGraph:Scopes")
    .Get<List<string>>();
builder.Services.AddGraphClient(baseUrl, scopes);

builder.Services.AddHttpClient("SecureAPIClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["SecureApi:BaseUrl"] ?? builder.HostEnvironment.BaseAddress);
}).AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SecureAPIClient"));

builder.Services.AddMsalAuthentication<RemoteAuthenticationState, CustomUserAccount>(options =>
    {
        options.ProviderOptions.DefaultAccessTokenScopes.Add("https://graph.microsoft.com/User.Read");
        //options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
        //options.ProviderOptions.DefaultAccessTokenScopes.Add("offline_access");
        options.ProviderOptions.LoginMode = "redirect";
        options.UserOptions.RoleClaim = "roles";
        builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    })
    .AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, CustomUserAccount, CustomAccountFactory>();

await builder.Build().RunAsync();