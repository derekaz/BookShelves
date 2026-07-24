using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Shared.Services;
using BookShelves.Shared.Services.AuthorizationPolicies;
using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Web.Components;
using BookShelves.Web.Services;
using BookShelves.Web.Shared.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
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

// builder.Services.AddMicrosoftIdentityConsentHandler();

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

builder.Services.AddScoped<IBook, Book>();
builder.Services.AddScoped<IWeatherForecaster, ServerWeatherForecaster>();
builder.Services.AddScoped<IBookFactory, ServerBookFactory>();
builder.Services.AddScoped<IBooksDataService, ServerBooksDataService>();
builder.Services.AddScoped<IAuthorDataService, ServerAuthorsDataService>();

builder.Services.AddScoped<IFormFactor, ServerFormFactor>();
builder.Services.AddScoped<IVersionService, ServerVersionService>();
builder.Services.AddScoped<IAuthenticationUIProvider, WebAuthenticationUIProvider>();
builder.Services.AddTransient<ISyncDataService, ServerSyncDataService>();
builder.Services.AddTransient<ISyncProgressService, SyncProgressService>();

builder.Services.AddScoped<BearerTokenHandler>();
builder.Services.AddScoped<AuthorsDatasyncClientFactory>();

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddRazorPages();

// Define your proxy options cleanly here
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor
    | ForwardedHeaders.XForwardedProto
    | ForwardedHeaders.XForwardedHost
};
// Clear restrictions so it accepts headers from your local NGINX proxy/Docker networks
forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

var app = builder.Build();

// If the environment variable from Docker Compose is present, enforce it
if (builder.Configuration["ASPNETCORE_FORWARDEDHEADERS_ENABLED"] == "true")
{
    Console.WriteLine("Using forwarded headers middleware because ASPNETCORE_FORWARDEDHEADERS_ENABLED is set to true!");
    // Allows handling headers from reverse proxy containers on the internal network
    app.UseForwardedHeaders(forwardedHeadersOptions);
}

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

// app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
//app.UseWhen(context => !context.Request.Path.StartsWithSegments("/api"), appBuilder =>
//{
//    appBuilder.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
//});

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

app.MapControllers();
app.MapRazorPages();

app.MapGet("/account/login", (string returnUrl, HttpContext context) =>
{
    // Challenge tells the OIDC/Entra middleware to start the browser redirect safely
    return Results.Challenge(
        properties: new AuthenticationProperties { RedirectUri = returnUrl },
        authenticationSchemes: [OpenIdConnectDefaults.AuthenticationScheme]
    );
});

app.MapGet("/MicrosoftIdentity/Account/Challenge", (string? returnUrl, HttpContext context) =>
{
    // Fall back to the root if no returnUrl was successfully passed
    var redirectUri = !string.IsNullOrEmpty(returnUrl) ? returnUrl : "/";

    // Trigger a true OpenID Connect challenge to Entra ID
    return Results.Challenge(
        properties: new AuthenticationProperties { RedirectUri = redirectUri },
        authenticationSchemes: [OpenIdConnectDefaults.AuthenticationScheme]
    );
});

app.MapGet("/weatherforecast", ([FromServices] IWeatherForecaster WeatherForecaster) =>
{
    return WeatherForecaster.GetWeatherForecastAsync();
}).RequireAuthorization();

app.MapGet("/booksdata", async ([FromServices] IBooksDataService BooksDataService, HttpContext context) =>
{
    try
    {
        var books = await BooksDataService.GetBooksAsync();
        var xlatBooks = books.Select(b => Book.FromBookViewModel(b));
        return Results.Ok(xlatBooks);
    }
    catch (MicrosoftIdentityWebChallengeUserException)
    {
        return Results.Unauthorized();
    }
    catch (MsalUiRequiredException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        if (ex.InnerException?.Message.Contains("MsalUiRequiredException") == true)
        {
            return Results.Unauthorized();
        }
        return Results.InternalServerError();
    }
}).RequireAuthorization();

// POST endpoint to create a book via the server-side IBooksDataService implementation
app.MapPost("/booksdata", async ([FromServices] IBooksDataService BooksDataService, BookViewModel book) =>
{
    try
    {
        var result = await BooksDataService.CreateBookAsync(book);
        return result ? Results.Ok() : Results.StatusCode(500);
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
}).RequireAuthorization();

// DELETE endpoint to delete a book via the server-side IBooksDataService implementation
app.MapDelete("/booksdata/{id}", async ([FromServices] IBooksDataService BooksDataService, string id) =>
{
    try
    {
        var book = new BookViewModel { Id = id };
        var result = await BooksDataService.DeleteBookAsync(book);
        return result ? Results.Ok() : Results.StatusCode(500);
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
}).RequireAuthorization();

// PUT endpoint to update a book via the server-side IBooksDataService implementation
app.MapPut("/booksdata/{id}", async ([FromServices] IBooksDataService BooksDataService, string id, BookViewModel book) =>
{
    try
    {
        // Ensure the id from route is set on the incoming book model
        book.Id = id;
        var result = await BooksDataService.UpdateBookAsync(book);
        return result ? Results.Ok() : Results.StatusCode(500);
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
}).RequireAuthorization();

app.MapGet("/authorsdata", async ([FromServices] IAuthorDataService AuthorsDataService, HttpContext context) =>
{
    try
    {
        var authors = await AuthorsDataService.GetAuthorsAsync();
        var xlatAuthors = authors.Select(a => Author.FromAuthorItemViewModel(a));
        return Results.Ok(xlatAuthors);
    }
    catch (MicrosoftIdentityWebChallengeUserException)
    {
        return Results.Unauthorized();
    }
    catch (MsalUiRequiredException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        if (ex.InnerException?.Message.Contains("MsalUiRequiredException") == true)
        {
            return Results.Unauthorized();
        }
        return Results.InternalServerError();
    }
}).RequireAuthorization();

// POST endpoint to create an author via the server-side IAuthorDataService implementation
app.MapPost("/authorsdata", async ([FromServices] IAuthorDataService AuthorsDataService, AuthorViewModel author) =>
{
    try
    {
        var result = await AuthorsDataService.CreateAuthorAsync(author);
        return result ? Results.Ok() : Results.StatusCode(500);
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
}).RequireAuthorization();

// DELETE endpoint to delete an author via the server-side IAuthorDataService implementation
app.MapDelete("/authorsdata/{id}", async ([FromServices] IAuthorDataService AuthorsDataService, string id) =>
{
    try
    {
        var author = new AuthorViewModel { Id = id };
        var result = await AuthorsDataService.DeleteAuthorAsync(author);
        return result ? Results.Ok() : Results.StatusCode(500);
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
}).RequireAuthorization();

// PUT endpoint to update an author via the server-side IAuthorDataService implementation
app.MapPut("/authorsdata/{id}", async ([FromServices] IAuthorDataService AuthorsDataService, string id, AuthorViewModel author) =>
{
    try
    {
        // Ensure the id from route is set on the incoming author model
        author.Id = id;
        var result = await AuthorsDataService.UpdateAuthorAsync(author);
        return result ? Results.Ok() : Results.StatusCode(500);
    }
    catch (Exception)
    {
        return Results.StatusCode(500);
    }
}).RequireAuthorization();

app.Run();
