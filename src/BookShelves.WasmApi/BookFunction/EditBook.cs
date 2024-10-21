using BlazorApp.Api.DataAccess;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
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

        [Function("EditBook")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = $"books/edit")] HttpRequestData req)
        {
            logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(EditBook)}");

            string? id = req.FunctionContext.BindingContext.BindingData["id"]!.ToString();
            string? title = req.FunctionContext.BindingContext.BindingData["title"]!.ToString();
            string? author = req.FunctionContext.BindingContext.BindingData["author"]!.ToString();

            string? requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic? data = JsonConvert.DeserializeObject(requestBody);
            id ??= data?.id;
            title ??= data?.title;
            author ??= data?.author;

            if (string.IsNullOrEmpty(id))
            {
                logger.LogInformation($"Unable to update a book with no id.");
                return req.CreateResponse(HttpStatusCode.UnprocessableEntity);
            }

            Book book = new()
            {
                Id = id,
                Title = title ?? string.Empty,
                Author = author ?? string.Empty
            };

            try
            {
                await bookRepository.UpdateAsync(id, book);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Unable to edit book: {book}");
                return req.CreateResponse(HttpStatusCode.UnprocessableEntity);
            }

            string responseMessage = $"Function triggered successfully and book edited. {book}";
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(responseMessage);
            return response;
        }
    }
}
