using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services.AuthorizationPolicies;
using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Web.Components;
using BookShelves.Web.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var initialScopes = builder.Configuration.GetSection("WeatherApi:Scopes").Get<string[]>();
var weatherApiConfig = builder.Configuration.GetSection("WeatherApi");
var booksApiConfig = builder.Configuration.GetSection("BooksApi");

builder.Services.AddMudServices();

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration, "AzureAd", OpenIdConnectDefaults.AuthenticationScheme)
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddDownstreamApi("WeatherApi", weatherApiConfig)
    .AddDownstreamApi("BooksApi", booksApiConfig)
    .AddDistributedTokenCaches();

//builder.Services.Configure<CookieAuthenticationOptions>(
//    CookieAuthenticationDefaults.AuthenticationScheme,
//    options =>
//    {
//        options.ExpireTimeSpan = TimeSpan.FromHours(1);
//        options.SlidingExpiration = true;
//    });

builder.Services.AddDistributedMemoryCache();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

builder.Services.AddMicrosoftIdentityConsentHandler();

// Add authorization services - auth state comes from server
builder.Services.AddAuthorization(options =>
{
    options.AddAppAuthorizationPolicies();
});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddHttpContextAccessor();

//builder.Services.AddMsalAuthentication(options =>
//{
//    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
//    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://graph.microsoft.com/User.Read");
//    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration["DownstreamApi:Scopes"]);
//});

//builder.Services.AddMicrosoftGraphClient("https://graph.microsoft.com/User.Read");

builder.Services.AddScoped<IWeatherForecaster, ServerWeatherForecaster>();
builder.Services.AddScoped<IBookFactory, ServerBookFactory>();
builder.Services.AddScoped<IBooksDataService, ServerBooksDataService>();

//builder.Services.AddHttpClient("ExternalApi",
//      client => client.BaseAddress = new Uri(builder.Configuration["ExternalApiUri"] ??
//          throw new Exception("Missing base address!")))
//      .AddHttpMessageHandler<TokenHandler>();

builder.Services.AddScoped<IFormFactor, ServerFormFactor>();
builder.Services.AddScoped<IVersionService, ServerVersionService>();
builder.Services.AddScoped<IAuthenticationUIProvider, WebAuthenticationUIProvider>();
builder.Services.AddTransient<IBooksSyncService, BooksSyncService>();

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();


app.MapRazorComponents<WebApp>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BookShelves.Shared._Imports).Assembly)
    .AddAdditionalAssemblies(typeof(BookShelves.Web.Client.Components._Imports).Assembly);

app.MapGet("/weatherforecast", ([FromServices] IWeatherForecaster WeatherForecaster) =>
{
    return WeatherForecaster.GetWeatherForecastAsync();
}).RequireAuthorization();

app.MapGet("/booksdata", ([FromServices] IBooksDataService BooksDataService) =>
{
    return BooksDataService.GetBooksAsync();
}).RequireAuthorization();

app.MapControllers();

app.Run();
