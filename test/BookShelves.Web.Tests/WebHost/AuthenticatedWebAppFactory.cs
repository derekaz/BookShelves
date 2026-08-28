using System.Security.Claims;
using System.Text.Encodings.Web;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BookShelves.Web.Tests.WebHost;

/// <summary>
/// <see cref="WebApplicationFactory{TEntryPoint}"/> variant that replaces the OIDC/cookie
/// auth stack with a lightweight test scheme and substitutes all three downstream data
/// services with in-process stubs. Downstream Datasync/WebApi calls and MSAL token
/// acquisition are never exercised, so tests can focus purely on the web-host endpoint layer.
/// </summary>
public sealed class AuthenticatedWebAppFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
                ["AzureAd:TenantId"] = "test-tenant",
                ["AzureAd:ClientId"] = "test-client-id",
                ["AzureAd:ClientSecret"] = "test-client-secret",
                ["AzureAd:CallbackPath"] = "/signin-oidc",
                ["WeatherApi:BaseUrl"] = "https://example.test/weather",
                ["WeatherApi:Scopes:0"] = "Weather.Get",
                ["BooksApi:BaseUrl"] = "https://example.test/books",
                ["BooksApi:Scopes:0"] = "Books.Read",
                ["BooksApi:Scopes:1"] = "Books.ReadWrite"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace the OIDC/cookie pipeline with a header-based test scheme so that
            // tests can inject an identity without a real token or browser redirect.
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = WebTestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = WebTestAuthHandler.SchemeName;
                options.DefaultScheme = WebTestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, WebTestAuthHandler>(WebTestAuthHandler.SchemeName, _ => { });

            // Replace downstream data services with stubs so no outbound HTTP or
            // MSAL token acquisition occurs during tests.
            services.AddScoped<IBooksDataService, StubBooksDataService>();
            services.AddScoped<IAuthorsDataService, StubAuthorsDataService>();
            services.AddScoped<IBookUserActionsDataService, StubBookUserActionsDataService>();
        });
    }

    // -------------------------------------------------------------------------
    // Test authentication handler
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates requests that carry <c>Authorization: Bearer test-token</c> and
    /// builds a <see cref="ClaimsPrincipal"/> from optional request headers:
    /// <list type="bullet">
    /// <item><c>X-Test-UserId</c> — defaults to <c>web-integration-test-user</c></item>
    /// <item><c>X-Test-Roles</c> — comma-separated role names</item>
    /// </list>
    /// </summary>
    internal sealed class WebTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string SchemeName = "WebTestAuth";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("Authorization", out var authHeader) ||
                !string.Equals(authHeader, "Bearer test-token", StringComparison.Ordinal))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var userId = Request.Headers.TryGetValue("X-Test-UserId", out var id)
                ? id.ToString()
                : "web-integration-test-user";

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, userId)
            };

            if (Request.Headers.TryGetValue("X-Test-Roles", out var roles))
            {
                foreach (var role in roles.ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var identity = new ClaimsIdentity(claims, SchemeName);
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    // -------------------------------------------------------------------------
    // Stub data services — return minimal well-formed data, never throw
    // -------------------------------------------------------------------------

    internal sealed class StubBooksDataService : IBooksDataService
    {
        public Task<IEnumerable<BookViewModel>> GetBooksAsync(bool includeSoftDeleted = false) =>
            Task.FromResult<IEnumerable<BookViewModel>>(
            [
                new BookViewModel { Id = "book-1", Title = "Test Book" }
            ]);

        public Task<bool> CreateBookAsync(BookViewModel book) => Task.FromResult(true);
        public Task<bool> UpdateBookAsync(BookViewModel book) => Task.FromResult(true);
        public Task<bool> DeleteBookAsync(BookViewModel book) => Task.FromResult(true);
    }

    internal sealed class StubAuthorsDataService : IAuthorsDataService
    {
        public Task<IEnumerable<AuthorViewModel>> GetAuthorsAsync(bool includeSoftDeleted = false) =>
            Task.FromResult<IEnumerable<AuthorViewModel>>(
            [
                new AuthorViewModel { Id = "author-1", Name = "Test Author" }
            ]);

        public Task<bool> CreateAuthorAsync(AuthorViewModel author) => Task.FromResult(true);
        public Task<bool> UpdateAuthorAsync(AuthorViewModel author) => Task.FromResult(true);
        public Task<bool> DeleteAuthorAsync(AuthorViewModel author) => Task.FromResult(true);
    }

    internal sealed class StubBookUserActionsDataService : IBookUserActionsDataService
    {
        public Task<IEnumerable<BookUserActionViewModel>> GetBookUserActionsAsync(bool includeSoftDeleted = false) =>
            Task.FromResult<IEnumerable<BookUserActionViewModel>>(
            [
                new BookUserActionViewModel
                {
                    Id = "action-1",
                    BookId = "book-1",
                    UserId = "web-integration-test-user",
                    ActionType = "ToBeRead"
                }
            ]);

        public Task<bool> CreateBookUserActionAsync(BookUserActionViewModel action)
        {
            var isValid = action is not null
                && !string.IsNullOrWhiteSpace(action.BookId)
                && !string.IsNullOrWhiteSpace(action.UserId)
                && !string.IsNullOrWhiteSpace(action.ActionType)
                && action.StartTimeUtc.HasValue
                && action.Details is not null;

            return Task.FromResult(isValid);
        }

        public Task<bool> UpdateBookUserActionAsync(BookUserActionViewModel action) => Task.FromResult(true);
        public Task<bool> DeleteBookUserActionAsync(BookUserActionViewModel action) => Task.FromResult(true);
    }
}
