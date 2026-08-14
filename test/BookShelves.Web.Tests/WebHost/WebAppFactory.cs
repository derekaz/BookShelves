using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace BookShelves.Web.Tests.WebHost;

public sealed class WebAppFactory : WebApplicationFactory<Program>
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
                ["BooksApi:Scopes:0"] = "Books.Read"
            });
        });
    }
}
