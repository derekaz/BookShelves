using BookShelves.WasmApi.DataAccess;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BookShelves.WasmApi;

public class Program
{
    public async static Task Main(string[] args)
    {
        var builder = FunctionsApplication.CreateBuilder(args);

        builder.ConfigureFunctionsWebApplication();

        builder.Services
            .AddApplicationInsightsTelemetryWorkerService()
            .ConfigureFunctionsApplicationInsights();

        //builder.Logging.Services.Configure<LoggerFilterOptions>(options =>
        //{
        //    // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
        //    // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/azure/azure-monitor/app/worker-service#ilogger-logs
        //    LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
        //        == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
        //    if (defaultRule is not null)
        //    {
        //        options.Rules.Remove(defaultRule);
        //    }
        //});

        builder.Services.AddTransient(x =>
        {
            IConfiguration? configuration = x.GetService<IConfiguration>();

            return new BookRepository(
                x.GetRequiredService<ILogger<BookRepository>>(),
                new CosmosClient(configuration!["CosmosDBConnectionString"]),
                "azmoore-westus2-db1",
                "azmoore-books-westus2-dbc1"
            );
        });

        builder.Services.AddTransient(x =>
        {
            IConfiguration? configuration = x.GetService<IConfiguration>();

            return new UniqueIdRepository(
                x.GetRequiredService<ILogger<UniqueIdRepository>>(),
                new CosmosClient(configuration!["CosmosDBConnectionString"]),
                "azmoore-westus2-db1",
                "azmoore-books-westus2-dbc1"
            );
        });

        var host = builder.Build();

        await host.RunAsync();

        //var host = new HostBuilder()
        //.ConfigureFunctionsWebApplication(/*builder =>
        //{
        //    builder
        //    .AddApplicationInsights()
        //    .AddApplicationInsightsLogger();

        //}*/)
        //.ConfigureServices(s =>
        //{
        //    s.AddTransient(x =>
        //    // s.AddSingleton(x =>
        //    {
        //        IConfiguration? configuration = x.GetService<IConfiguration>();

        //        return new BookRepository(
        //            x.GetRequiredService<ILogger<BookRepository>>(),
        //            new CosmosClient(configuration!["CosmosDBConnectionString"]),
        //            "azmoore-westus2-db1",
        //            "azmoore-books-westus2-dbc1"
        //        );
        //    });

        //    s.AddTransient(x =>
        //    // s.AddSingleton(x =>
        //    {
        //        IConfiguration? configuration = x.GetService<IConfiguration>();

        //        return new UniqueIdRepository(
        //            x.GetRequiredService<ILogger<UniqueIdRepository>>(),
        //            new CosmosClient(configuration!["CosmosDBConnectionString"]),
        //            "azmoore-westus2-db1",
        //            "azmoore-books-westus2-dbc1"
        //        );
        //    });
        //})
        //.Build();
        
        //await host.RunAsync();
    }
}
