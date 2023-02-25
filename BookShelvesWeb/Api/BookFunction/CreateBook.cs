using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Configuration;

using BlazorApp.Shared;

namespace BlazorApp.Api.BookFunction
{
    public class CreateBook
    {
        private readonly ILogger<CreateBook> logger;

        public CreateBook(ILogger<CreateBook> logger)
        {
            this.logger = logger;
        }

        [FunctionName("CreateBook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = $"books/new")] HttpRequest req,
            [CosmosDB(
                databaseName: "azmoore-westus2-db1",
                containerName: "azmoore-books-westus2-dbc1",
                Connection = "CosmosDbConnectionString")]IAsyncCollector<dynamic> documentsOut)
        {
            logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(CreateBook)}");

            string title = req.Query["title"];
            string author = req.Query["author"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            title ??= data?.title;
            author ??= data?.author;

            if (string.IsNullOrEmpty(title))
            {
                logger.LogInformation($"Unable to create book with no title.");
                return new UnprocessableEntityResult();
            }

            Book book = new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                Author = author
            };

            try
            {
                await documentsOut.AddAsync(book);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unable to add book: {book}");
                return new UnprocessableEntityResult();
            }

            string responseMessage = $"Function triggered successfully and book created. {book}";
            return new OkObjectResult(responseMessage);
        }

        [FunctionName("Book2")]
        public static async Task<IActionResult> Book2(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
