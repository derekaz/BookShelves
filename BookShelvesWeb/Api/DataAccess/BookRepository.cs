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
    public class BookRepository
    {
        private readonly ILogger<BookRepository> logger;
        private readonly Container bookContainer;

        public BookRepository(
            ILogger<BookRepository> logger,
            CosmosClient client, 
            string databaseName, 
            string containerName
            )
        {
            this.logger = logger;
            bookContainer = client.GetContainer(databaseName, containerName);
        }

        public FeedIterator<Book> ReadAllBooks()
        {
            logger.LogInformation("Getting all books from DB");

            QueryDefinition queryDefinition = new QueryDefinition(
                "SELECT * FROM c");

            return bookContainer.GetItemQueryIterator<Book>(queryDefinition);
        }

        public FeedIterator<Book> ReadAllBooksWithTitleTerm(string title)
        {
            logger.LogInformation("Getting books from DB with title");

            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            logger.LogInformation($"Searching for: {title}");

            QueryDefinition queryDefinition = new QueryDefinition(
                "SELECT * FROM items i WHERE CONTAINS(i.title, @searchterm)")
                .WithParameter("@searchterm", title);

            return bookContainer.GetItemQueryIterator<Book>(queryDefinition);
        }
    }
}
