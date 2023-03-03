using BlazorApp.Shared;
using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using BlazorApp.Api.BookFunction;
using Microsoft.Extensions.Logging;
using static System.Reflection.Metadata.BlobBuilder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace BlazorApp.Api.DataAccess
{
    public class Books
    {
        private readonly ILogger<CreateBook> logger;
        private readonly CosmosClient cosmosClient;

        public Books(
            ILogger<CreateBook> logger, 
            [CosmosDB(
                databaseName: "azmoore-westus2-db1",
                containerName: "azmoore-books-westus2-dbc1",
                Connection = "CosmosDbConnectionString")] 
            CosmosClient client
            )
        {
            this.logger = logger;
            cosmosClient = client;
        }

        //public IEnumerable<Book> GetBooks([CosmosDB(
        //        databaseName: "azmoore-westus2-db1",
        //        containerName: "azmoore-books-westus2-dbc1",
        //        Connection = "CosmosDbConnectionString",
        //        SqlQuery = "SELECT * FROM c")] IEnumerable<Book> books
        //    )
        //{
        //    logger.LogInformation($"Getting all books from DB");

        //    return books;
        //}
        public FeedIterator<Book> ReadAllBooks()
        {
            logger.LogInformation("Getting all books from DB");

            Container container = cosmosClient.GetDatabase("azmoore-westus2-db1").GetContainer("azmoore-books-westus2-dbc1");

            QueryDefinition queryDefinition = new QueryDefinition(
                "SELECT * FROM c");

            return container.GetItemQueryIterator<Book>(queryDefinition);
        }

        public FeedIterator<Book> ReadAllBooksWithTitleTerm(string title)
        {
            logger.LogInformation("Getting books from DB with title");

            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            Container container = cosmosClient.GetDatabase("azmoore-westus2-db1").GetContainer("azmoore-books-westus2-dbc1");

            logger.LogInformation($"Searching for: {title}");

            QueryDefinition queryDefinition = new QueryDefinition(
                "SELECT * FROM items i WHERE CONTAINS(i.title, @searchterm)")
                .WithParameter("@searchterm", title);

            return container.GetItemQueryIterator<Book>(queryDefinition);
        }
    }
}
