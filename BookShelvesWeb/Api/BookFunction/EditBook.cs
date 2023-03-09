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
    public class EditBook
    {
        private readonly ILogger<EditBook> logger;
        private readonly BookRepository bookRepository;

        public EditBook(ILogger<EditBook> logger, BookRepository bookRepository)
        {
            this.logger = logger;
            this.bookRepository = bookRepository;
        }

        [FunctionName("EditBook")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = $"books/edit")] HttpRequest req) //,
        {
            logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(EditBook)}");

            string id = req.Query["id"];
            string title = req.Query["title"];
            string author = req.Query["author"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            id ??= data?.id;
            title ??= data?.title;
            author ??= data?.author;

            if (string.IsNullOrEmpty(id))
            {
                logger.LogInformation($"Unable to update a book with no id.");
                return new UnprocessableEntityResult();
            }

            Book book = new()
            {
                Id = id,
                Title = title,
                Author = author
            };

            try
            {
                await bookRepository.UpdateAsync(id, book);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                logger.LogError(ex, $"Unable to edit book: {book}");
                return new UnprocessableEntityResult();
            }

            string responseMessage = $"Function triggered successfully and book edited. {book}";
            return new OkObjectResult(responseMessage);
        }
    }
}
