using BookShelves.WasmSwa;
using BookShelves.WasmSwa.Data;
using BookShelves.Shared;
using BookShelves.Shared.DataInterfaces;
using BookShelves.WebShared.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookShelves.WasmSwa.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<WasmApp>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddRazorClassLibraryServices();

builder.Services.AddSingleton<IVersionService, VersionService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddTransient<IBooksDataService, BooksDataService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IGraphService, GraphService>();
builder.Services.AddCascadingAuthenticationState();

var baseUrl = string.Join("/",
    builder.Configuration.GetSection("MicrosoftGraph")["BaseUrl"],
    builder.Configuration.GetSection("MicrosoftGraph")["Version"]);
var scopes = builder.Configuration.GetSection("MicrosoftGraph:Scopes")
    .Get<List<string>>();
builder.Services.AddGraphClient(baseUrl, scopes);

builder.Services.AddMsalAuthentication<RemoteAuthenticationState, CustomUserAccount>(options =>
    {
        options.UserOptions.RoleClaim = "roles";
        builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    })
    .AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, CustomUserAccount, CustomAccountFactory>();

await builder.Build().RunAsync();