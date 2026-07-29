using BookShelves.WasmApi.DataAccess;
using BookShelves.WasmApi.Utilities;
using BookShelves.Web.Shared.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Transactions;

namespace BookShelves.WasmApi.BookFunction;

public class CreateBook
{
    private readonly ILogger<CreateBook> _logger;
    private readonly BookRepository _bookRepository;

    public CreateBook(ILogger<CreateBook> logger, BookRepository bookRepository)
    {
        _logger = logger;
        _bookRepository = bookRepository;
    }

    [Function("CreateBook")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = $"books/new")] HttpRequestData req)
    {
        _logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(Run)}");
        string? title = req.FunctionContext.BindingContext.BindingData["title"]!.ToString();
        string? author = req.FunctionContext.BindingContext.BindingData["author"]!.ToString();

        string? requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        JsonNode? jsonNode = JsonNode.Parse(requestBody);
        if (jsonNode is JsonObject jsonObject)
        {
            title ??= (string?)jsonObject["title"];
            author ??= (string?)jsonObject["author"];
        }

        if (string.IsNullOrEmpty(title))
        {
            _logger.LogInformation($"Unable to create book with no title.");
            return req.CreateResponse(HttpStatusCode.UnprocessableEntity);
        }

        Book? newBook;

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            newBook = new()
            {
                Id = Guid.CreateVersion7().ToString(),
                Title = title ?? string.Empty,
                AuthorId = author ?? string.Empty
            };

            try
            {
                await _bookRepository.AddAsync(newBook);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to add book: {newBook}");
                return req.CreateResponse(HttpStatusCode.UnprocessableEntity);
            }

            scope.Complete();
        }

        string responseMessage = $"Function triggered successfully and book created. {newBook}";
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(responseMessage);

        return response;
    }

    [Function("CreateBook-v2")]
    public async Task<HttpResponseData> CreateBookV2(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = $"v2/books/new")] HttpRequestData req)
    {
        _logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(CreateBookV2)}");
        string? title = req.FunctionContext.BindingContext.BindingData["title"]!.ToString();
        string? author = req.FunctionContext.BindingContext.BindingData["author"]!.ToString();
        string? lastUpdateTime = req.FunctionContext.BindingContext.BindingData["lastUpdateTime"]!.ToString();

        string? requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        //dynamic? data = Newtonsoft.Json.JsonConvert.DeserializeObject(requestBody);
        //title ??= data?.title;
        //author ??= data?.author;
        //lastUpdateTime ??= data?.lastUpdateTime;

        JsonNode? jsonNode = JsonNode.Parse(requestBody);
        if (jsonNode is JsonObject jsonObject)
        {
            title ??= (string?)jsonObject["title"];
            author ??= (string?)jsonObject["author"];
            lastUpdateTime ??= (string?)jsonObject["lastUpdateTime"];
        }

        string responseMessage;

        if (string.IsNullOrEmpty(title))
        {
            responseMessage = $"Unable to create book without a title.";
            _logger.LogInformation(responseMessage);

            return await ResponseFactory.CreateFailedResponseNoContentAsync(req, HttpStatusCode.UnprocessableEntity, responseMessage, null, responseMessage);
        }

        Book? newBook;

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            newBook = new()
            {
                Id = Guid.CreateVersion7().ToString(),
                Title = title ?? string.Empty,
                AuthorId = author ?? string.Empty
            };

            try
            {
                await _bookRepository.AddAsync(newBook);
            }
            catch (Exception ex)
            {
                responseMessage = $"Unable to add book: {newBook}";
                _logger.LogError(ex, responseMessage);

                return await ResponseFactory.CreateFailedResponseAsync<Book>(req, newBook, HttpStatusCode.UnprocessableEntity, responseMessage, ex);
            }

            scope.Complete();
        }

        responseMessage = $"Function triggered successfully and book created.";
        return await ResponseFactory.CreateSuccessResponseAsync<Book>(req, responseMessage, newBook);
    }
}
