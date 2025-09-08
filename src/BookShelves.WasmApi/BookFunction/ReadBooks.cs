using BookShelves.WasmApi.DataAccess;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using System.Security.Policy;
using System;

namespace BookShelves.WasmApi.BookFunction;

public class ReadBooks
{
    private readonly ILogger<ReadBooks> logger;
    private readonly BookRepository booksData;

    public ReadBooks(ILogger<ReadBooks> logger, BookRepository booksData)
    {
        this.logger = logger;
        this.booksData = booksData;
    }

    [Function("ReadBooks1")]
    public async Task<HttpResponseData> ReadAllBooks(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = $"books")] HttpRequestData req
        )
    {
        logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(ReadBooks)}");

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(await booksData.GetMultipleAsync("SELECT * FROM c"));
        return response;
    }

    [Function("ReadBooks2")]
    public async Task<HttpResponseData> ReadAllBooksWithTitleTerm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = $"books/title")] HttpRequestData req
        )
    {
        logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(ReadAllBooksWithTitleTerm)}");
        var searchterm = req.FunctionContext.BindingContext.BindingData["title"]!.ToString();
        if (string.IsNullOrWhiteSpace(searchterm))
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(await booksData.GetMultipleAsync($"SELECT * FROM items i WHERE CONTAINS '{searchterm}'"));
        return response;
    }

    [Function("ReadBooks3")]
    public async Task<HttpResponseData> ReadAllBooksWithAuthorTerm(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = $"books/author")] HttpRequestData req
        )
    {
        logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(ReadAllBooksWithAuthorTerm)}");
        var searchterm = req.FunctionContext.BindingContext.BindingData["author"]!.ToString();
        if (string.IsNullOrWhiteSpace(searchterm))
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(await booksData.GetMultipleAsync($"SELECT * FROM items i WHERE CONTAINS '{searchterm}'"));
        return response;
    }

    [Function("ReadBooks4")]
    public async Task<HttpResponseData> ReadAllBooksUpdatedSinceLastSync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = $"books/sync")] HttpRequestData req
        )
    {
        logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(ReadAllBooksUpdatedSinceLastSync)}");
        var searchterm = req.FunctionContext.BindingContext.BindingData["lastSyncTime"]!.ToString();
        DateTime searchValue = DateTime.MinValue;
        if (string.IsNullOrWhiteSpace(searchterm))
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var parseResult = DateTime.TryParse(req.FunctionContext.BindingContext.BindingData["lastSyncTime"]!.ToString(), out searchValue);
        if (!parseResult)
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var searchString = searchValue.ToString("o");
        if (string.IsNullOrWhiteSpace(searchString))
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(await booksData.GetMultipleAsync($"SELECT * FROM items i WHERE i.lastUpdateTime > '{searchString}' ORDER BY i.lastUpdateTime ASC"));
        return response;
    }
}
