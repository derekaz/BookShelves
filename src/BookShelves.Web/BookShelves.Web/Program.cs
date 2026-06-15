//using BookShelves.Shared;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Web;
using BookShelves.Web.Components;
using BookShelves.Web.Services;
//using BookShelves.WebShared.Data;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
//using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

const string MS_OIDC_SCHEME = "MicrosoftOidc";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(MS_OIDC_SCHEME)
    .AddOpenIdConnect(MS_OIDC_SCHEME, oidcOptions =>
    {
        var oidcConfig = builder.Configuration.GetSection("AzureSettings");
        //oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        oidcOptions.SignInScheme = OpenIdConnectDefaults.AuthenticationScheme;
        oidcOptions.Scope.Add(OpenIdConnectScope.OpenIdProfile);
        builder.Configuration.GetSection("AzureAD:Scopes").GetChildren().ToList().ForEach(scope =>
        {
            if (scope.Value != null)
            {
                oidcOptions.Scope.Add(scope.Value);
            }
        });
        //oidcOptions.CallbackPath = new PathString("/signin-oidc");
        //oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
        //oidcOptions.RemoteSignOutPath = new PathString("/signout-oidc");
        oidcOptions.Authority = builder.Configuration["AzureAD:Authority"];
        oidcOptions.ClientId = builder.Configuration["AzureAD:ClientId"];
        oidcOptions.ClientSecret = builder.Configuration["AzureAD:ClientSecret"];
        oidcOptions.ResponseType = "code";
        oidcOptions.MapInboundClaims = false;
        oidcOptions.TokenValidationParameters.NameClaimType = "name";
        oidcOptions.TokenValidationParameters.RoleClaimType = "roles";
        //var microsoftIssuerValidator = AadIssuerValidator.GetAadIssuerValidator(oidcOptions.Authority);
        //oidcOptions.TokenValidationParameters.IssuerValidator = microsoftIssuerValidator.Validate;

        // builder.Configuration.Bind("Authentication:Microsoft", oidcOptions);
        //oidcOptions.SaveTokens = true;
    })
    //.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
    .AddCookie(OpenIdConnectDefaults.AuthenticationScheme); //  CookieAuthenticationDefaults.AuthenticationScheme);

//builder.Services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, MS_OIDC_SCHEME);
builder.Services.ConfigureCookieOidc(OpenIdConnectDefaults.AuthenticationScheme, MS_OIDC_SCHEME);

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

// builder.Services.AddRazorClassLibraryServices();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<BookShelves.Web.TokenHandler>();
//builder.Services.AddScoped<BookShelves.Web.TokenHandler>(sp =>
//{
//{
//    var authorizationMessageHandler = sp.GetRequiredService<AuthorizationMessageHandler>();
//    authorizationMessageHandler.InnerHandler = new HttpClientHandler();
//    authorizationMessageHandler = authorizationMessageHandler.ConfigureHandler(
//        authorizedUrls: new[] { builder.Configuration["DownstreamApi:BaseUrl"] },
//        scopes: new[] { builder.Configuration["DownstreamApi:Scopes"] });
//    return new HttpClient(authorizationMessageHandler)
//    {
//        BaseAddress = new Uri(builder.Configuration["DownstreamApi:BaseUrl"] ?? string.Empty)
//    };
//}});

//builder.Services.AddMsalAuthentication(options =>
//{
//    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
//    options.ProviderOptions.DefaultAccessTokenScopes.Add("https://graph.microsoft.com/User.Read");
//    options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration["DownstreamApi:Scopes"]);
//});

//builder.Services.AddMicrosoftGraphClient("https://graph.microsoft.com/User.Read");

builder.Services.AddScoped<IWeatherForecaster, ServerWeatherForecaster>();

builder.Services.AddHttpClient("ExternalApi",
      client => client.BaseAddress = new Uri(builder.Configuration["ExternalApiUri"] ??
          throw new Exception("Missing base address!")))
      .AddHttpMessageHandler<TokenHandler>();

builder.Services.AddScoped<IVersionService, VersionService>();
builder.Services.AddScoped<IAuthenticationUIProvider, WebAuthenticationUIProvider>();
//builder.Services.AddScoped(sp =>
//        new HttpClient
//        { BaseAddress = new Uri(builder.Configuration["API_Uri"] ?? builder.Configuration["API_Prefix"] ?? builder.Environment.WebRootPath) }
//    );
// builder.Services.AddTransient<IBook, Book>();
// builder.Services.AddTransient<IBookFactory, BookFactory>();
// builder.Services.AddScoped<IBooksDataService, BooksDataService>();
builder.Services.AddTransient<IBooksSyncService, BooksSyncService>();

builder.Services.AddScoped<IAuthService, AuthService>();

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
app.UseAntiforgery();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapRazorComponents<WebApp>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BookShelves.Shared._Imports).Assembly)
    .AddAdditionalAssemblies(typeof(BookShelves.Web.Client.Components._Imports).Assembly);
// .AddAdditionalAssemblies(typeof(BookShelves.Shared._Imports).Assembly);

// app.MapGroup("/authentication").MapLoginAndLogout();

app.MapGet("/weatherforecast", ([FromServices] IWeatherForecaster WeatherForecaster) =>
{
    return WeatherForecaster.GetWeatherForecastAsync();
}); //.RequireAuthorization();

app.MapControllers();

app.Run();
