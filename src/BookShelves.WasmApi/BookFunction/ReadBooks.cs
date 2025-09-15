using BookShelves.WasmApi.DataAccess;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using System;
using BookShelves.WebShared.Data;
using BookShelves.WasmApi.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        await response.WriteAsJsonAsync(await booksData.GetMultipleAsync($"SELECT * FROM c WHERE c.id <> '{Book.BOOKS_UNIQUEID_RECORD_ID}'"));
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
            return await ResponseFactory.CreateFailedResponseNoContentAsync(req, HttpStatusCode.NotFound, "Query not defined.");
            // return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var parseResult = DateTime.TryParse(req.FunctionContext.BindingContext.BindingData["lastSyncTime"]!.ToString(), out searchValue);
        if (!parseResult)
        {
            return await ResponseFactory.CreateFailedResponseNoContentAsync(req, HttpStatusCode.NotFound, "Query parameter lastSyncTime not found or invalid format.");
            // return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var searchString = searchValue.ToString("o");
        if (string.IsNullOrWhiteSpace(searchString))
        {
            return await ResponseFactory.CreateFailedResponseNoContentAsync(req, HttpStatusCode.NotFound, "Query parameter lastSyncTime invalid format.");
            // return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var books = await booksData.GetMultipleAsync($"SELECT * FROM items i WHERE i.id <> '{Book.BOOKS_UNIQUEID_RECORD_ID}' AND i.lastUpdateTime > '{searchString}' ORDER BY i.lastUpdateTime ASC");
        var response = await ResponseFactory.CreateSuccessResponseAsync<List<Book>>(req, "Records returned.", [.. books]);

        //var response = req.CreateResponse(HttpStatusCode.OK);
        //await response.WriteAsJsonAsync(books);

        return response;
    }
}
