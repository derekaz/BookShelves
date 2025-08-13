using BookShelves.WasmApi.DataAccess;
using Microsoft.Azure.Cosmos;
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

        var host = new HostBuilder()
        .ConfigureFunctionsWebApplication(/*builder =>
        {
            builder
            .AddApplicationInsights()
            .AddApplicationInsightsLogger();

        }*/)
        .ConfigureServices(s =>
        {
            s.AddSingleton(x =>
            {
                IConfiguration? configuration = x.GetService<IConfiguration>();

                return new BookRepository(
                    x.GetRequiredService<ILogger<BookRepository>>(),
                    new CosmosClient(configuration!["CosmosDBConnectionString"]),
                    "azmoore-westus2-db1",
                    "azmoore-books-westus2-dbc1"
                );
            });
        })
        .Build();
        
        await host.RunAsync();
    }
}
