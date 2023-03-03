using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using System.Linq;
using BlazorApp.Api.DataAccess;

namespace BlazorApp.Api.BookFunction
{
    public class ReadBooks
    {
        private readonly ILogger<CreateBook> logger;

        public ReadBooks(ILogger<CreateBook> logger)
        {
            this.logger = logger;
        }

        [FunctionName("ReadBooks1")]
        public async Task<IActionResult> ReadAllBooks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = $"books")] HttpRequest req, 
            Books booksData
            //[CosmosDB(
            //    databaseName: "azmoore-westus2-db1",
            //    containerName: "azmoore-books-westus2-dbc1",
            //    Connection = "CosmosDbConnectionString",
            //    SqlQuery = "SELECT * FROM c")] IEnumerable<Book> books
            )
        {
            logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(ReadBooks)}");

            List<Book> books = new();
            using (FeedIterator<Book> resultSet = booksData.ReadAllBooks())
            {
                if (resultSet == null)
                {
                    return new BadRequestResult();
                }

                while (resultSet.HasMoreResults)
                {
                    FeedResponse<Book> response = await resultSet.ReadNextAsync();
                    foreach (Book item in response)
                    {
                        logger.LogInformation(item.Title);
                        books.Add(item);
                    }
                }
            }

            return new OkObjectResult(books);

            //if (booksData.GetBooks() is null)
            //{
            //    return new BadRequestResult();
            //}

            //foreach (var book in booksData.GetBooks())
            //{
            //    logger.LogInformation(book.Title);
            //}

            //return new OkObjectResult(books);
        }

        [FunctionName("ReadBooks2")]
        public async Task<IActionResult> ReadAllBooksWithTitleTerm(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = $"books/title")] HttpRequest req,
            Books booksData //,
            //[CosmosDB(
            //    databaseName: "azmoore-westus2-db1",
            //    containerName: "azmoore-books-westus2-dbc1",
            //    Connection = "CosmosDbConnectionString")] CosmosClient client
            )
        {
            logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(ReadAllBooksWithTitleTerm)}");

            var searchterm = req.Query["title"].ToString();
            if (string.IsNullOrWhiteSpace(searchterm))
            {
                return new NotFoundResult();
            }

            List<Book> books = new(); 
            using (FeedIterator<Book> resultSet = booksData.ReadAllBooksWithTitleTerm(searchterm))
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<Book> response = await resultSet.ReadNextAsync();
                    foreach (Book item in response)
                    {
                        logger.LogInformation(item.Title);
                        books.Add(item);
                    }
                }
            }

            return new OkObjectResult(books);

            //if (string.IsNullOrWhiteSpace(searchterm))
            //{
            //    return (ActionResult)new NotFoundResult();
            //}

            //Container container = client.GetDatabase("azmoore-westus2-db1").GetContainer("azmoore-books-westus2-dbc1");

            //logger.LogInformation($"Searching for: {searchterm}");

            //QueryDefinition queryDefinition = new QueryDefinition(
            //    "SELECT * FROM items i WHERE CONTAINS(i.title, @searchterm)")
            //    .WithParameter("@searchterm", searchterm);

            //List<Book> books = new();

            //using (FeedIterator<Book> resultSet = container.GetItemQueryIterator<Book>(queryDefinition))
            //{
            //    while (resultSet.HasMoreResults)
            //    {
            //        FeedResponse<Book> response = await resultSet.ReadNextAsync();
            //        foreach(Book item in response)
            //        {
            //            logger.LogInformation(item.Title);
            //            books.Add(item);
            //        }
            //        //Book item = response.First();
            //    }
            //}

            //return new OkObjectResult(books);
        }
    }
}
