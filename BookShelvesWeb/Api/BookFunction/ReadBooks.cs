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
using Microsoft.AspNetCore.Mvc.Formatters.Internal;

namespace BlazorApp.Api.BookFunction
{
    public class ReadBooks
    {
        private readonly ILogger<CreateBook> logger;
        private readonly BookRepository booksData;

        public ReadBooks(ILogger<CreateBook> logger, BookRepository booksData)
        {
            this.logger = logger;
            this.booksData = booksData;
        }

        [FunctionName("ReadBooks1")]
        public async Task<IActionResult> ReadAllBooks(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = $"books")] HttpRequest req
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
        }

        [FunctionName("ReadBooks2")]
        public async Task<IActionResult> ReadAllBooksWithTitleTerm(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = $"books/title")] HttpRequest req
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
        }
    }
}
