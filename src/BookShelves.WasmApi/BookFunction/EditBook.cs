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

namespace BookShelves.WasmApi.BookFunction;

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

        string? id = req.FunctionContext.BindingContext.BindingData["idValue"]!.ToString();
        string? title = req.FunctionContext.BindingContext.BindingData["title"]!.ToString();
        string? author = req.FunctionContext.BindingContext.BindingData["author"]!.ToString();

        string? requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        JsonNode? jsonNode = JsonNode.Parse(requestBody);
        if (jsonNode is JsonObject jsonObject)
        {
            id ??= (string?)jsonObject["id"];
            title ??= (string?)jsonObject["title"];
            author ??= (string?)jsonObject["author"];
        }


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

    [Function("EditBook-v2")]
    public async Task<HttpResponseData> EditBookV2(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = $"v2/books/edit")] HttpRequestData req)
    {
        logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(EditBook)}");

        string? id = req.FunctionContext.BindingContext.BindingData["idValue"]!.ToString();
        string? title = req.FunctionContext.BindingContext.BindingData["title"]!.ToString();
        string? author = req.FunctionContext.BindingContext.BindingData["author"]!.ToString();
        string? lastUpdateTime = req.FunctionContext.BindingContext.BindingData["lastUpdateTime"]!.ToString();

        string? requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        JsonNode? jsonNode = JsonNode.Parse(requestBody);
        if (jsonNode is JsonObject jsonObject)
        {
            id ??= (string?)jsonObject["id"];
            title ??= (string?)jsonObject["title"];
            author ??= (string?)jsonObject["author"];
            lastUpdateTime ??= (string?)jsonObject["lastUpdateTime"];
        }

        string responseMessage;

        if (string.IsNullOrEmpty(id))
        {
            responseMessage = $"Unable to update a book without an id.";
            logger.LogInformation(responseMessage);

            return await ResponseFactory.CreateFailedResponseNoContentAsync(req, HttpStatusCode.UnprocessableEntity, responseMessage, null, responseMessage);
        }

        Book book = new()
        {
            Id = id,
            Title = title ?? string.Empty,
            Author = author ?? string.Empty,
            LastUpdateTime = string.IsNullOrEmpty(lastUpdateTime) ? DateTime.UtcNow : DateTime.Parse(lastUpdateTime)
        };

        try
        {
            await bookRepository.UpdateAsync(id, book);
        }
        catch (Exception ex)
        {
            responseMessage = $"Unable to edit book: {book}";
            logger.LogError(ex, responseMessage);

            return await ResponseFactory.CreateFailedResponseAsync<Book>(req, book, HttpStatusCode.UnprocessableEntity, responseMessage);
        }

        responseMessage = $"Function triggered successfully and book edited.";

        return await ResponseFactory.CreateSuccessResponseAsync<Book>(req, responseMessage, book);
    }
}
