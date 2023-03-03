using BlazorApp.Api.DataAccess;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
[assembly: FunctionsStartup(typeof(BlazorApp.Api.Startup))]

namespace BlazorApp.Api
{
    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<Books>();
            builder.Services.AddCosmosRepository()
            //throw new NotImplementedException();
        }

        private static async Task<Books> InitializeBooksClientAsync()
        {
            var client = new CosmosClient("CosmosDbConnectionString", clientOptions)

                CosmosDB(
                databaseName: "azmoore-westus2-db1",
                containerName: "azmoore-books-westus2-dbc1",
                Connection = "CosmosDbConnectionString")
        }
    }
}
