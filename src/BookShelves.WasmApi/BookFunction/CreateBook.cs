using BookShelves.WasmApi.DataAccess;
using BookShelves.WebShared.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BookShelves.WasmApi.BookFunction;

public class CreateBook
{
    private readonly ILogger<CreateBook> logger;
    private readonly BookRepository bookRepository;

    public CreateBook(ILogger<CreateBook> logger, BookRepository bookRepository)
    {
        this.logger = logger;
        this.bookRepository = bookRepository;
    }

    [Function("CreateBook")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = $"books/new")] HttpRequestData req)
    {
        logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(Run)}");
        string? title = req.FunctionContext.BindingContext.BindingData["title"]!.ToString();
        string? author = req.FunctionContext.BindingContext.BindingData["author"]!.ToString();

        string? requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic? data = JsonConvert.DeserializeObject(requestBody);
        title ??= data?.title;
        author ??= data?.author;

        if (string.IsNullOrEmpty(title))
        {
            logger.LogInformation($"Unable to create book with no title.");
            return req.CreateResponse(HttpStatusCode.UnprocessableEntity);
        }

        Book book = new()
        {
            Id = Guid.NewGuid().ToString(),
            Title = title ?? string.Empty,
            Author = author ?? string.Empty
        };

        try
        {
            await bookRepository.AddAsync(book);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Unable to add book: {book}");
            return req.CreateResponse(HttpStatusCode.UnprocessableEntity);
        }

        string responseMessage = $"Function triggered successfully and book created. {book}";
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(responseMessage);

        return response;
    }
}
