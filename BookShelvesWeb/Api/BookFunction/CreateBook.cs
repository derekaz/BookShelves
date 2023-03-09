using BlazorApp.Api.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlazorApp.Api.BookFunction
{
    public class CreateBook
    {
        private readonly ILogger<CreateBook> logger;
        private readonly BookRepository bookRepository;

        public CreateBook(ILogger<CreateBook> logger, BookRepository bookRepository)
        {
            this.logger = logger;
            this.bookRepository = bookRepository;
        }

        [FunctionName("CreateBook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = $"books/new")] HttpRequest req) //,
        {
            logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(Run)}");

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
                await bookRepository.AddAsync(book);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unable to add book: {book}");
                return new UnprocessableEntityResult();
            }

            string responseMessage = $"Function triggered successfully and book created. {book}";
            return new OkObjectResult(responseMessage);
        }
    }
}
