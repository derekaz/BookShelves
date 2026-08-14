using Microsoft.AspNetCore.Hosting;

namespace BookShelves.WebApi.Tests.Auth;

internal static class TestHostConfiguration
{
    internal const string CosmosConnectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=dGVzdC1rZXktdGVzdC1rZXktdGVzdC1rZXktdGVzdC1rZXk=;";

    public static void Apply(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:CosmosDBConnectionString", CosmosConnectionString);
        builder.UseSetting("AzureAd:Instance", "https://login.microsoftonline.com/");
        builder.UseSetting("AzureAd:TenantId", "test-tenant");
        builder.UseSetting("AzureAd:ClientId", "test-client");

        Environment.SetEnvironmentVariable("ConnectionStrings__CosmosDBConnectionString", CosmosConnectionString);
        Environment.SetEnvironmentVariable("AzureAd__Instance", "https://login.microsoftonline.com/");
        Environment.SetEnvironmentVariable("AzureAd__TenantId", "test-tenant");
        Environment.SetEnvironmentVariable("AzureAd__ClientId", "test-client");
    }
}
