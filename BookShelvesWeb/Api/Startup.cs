using BlazorApp.Api.DataAccess;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
[assembly: FunctionsStartup(typeof(BlazorApp.Api.Startup))]

namespace BlazorApp.Api
{
    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            string config = builder.GetContext().Configuration["CosmosDBConnectionString"];

            builder.Services.AddSingleton<BookRepository>(x =>
                new BookRepository(
                    x.GetRequiredService<ILogger<BookRepository>>(),
                    new CosmosClient(config),
                    "azmoore-westus2-db1",
                    "azmoore-books-westus2-dbc1"
                    )
                );
        }
    }
}
