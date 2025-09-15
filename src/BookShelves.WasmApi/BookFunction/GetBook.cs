using BookShelves.WasmApi.DataAccess;
using BookShelves.WasmApi.Utilities;
using BookShelves.WebShared.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace BookShelves.WasmApi.BookFunction;

public class GetBook
{
    private readonly ILogger<GetBook> _logger;
    private readonly BookRepository _booksData;

    public GetBook(ILogger<GetBook> logger, BookRepository booksData)
    {
        _logger = logger;
        _booksData = booksData;
    }

    [Function("GetBook1")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "book/{id}")] HttpRequestData req, 
        string id
    )
    {
        _logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(Run)} with id:{id}");

        Book? book;
        string responseMessage;

        try
        {
            book = await _booksData.GetAsync(id);
        }
        catch (Exception ex)
        {
            responseMessage = $"Unable to retrieve book: {id}";
            _logger.LogError(ex, responseMessage);

            return await ResponseFactory.CreateFailedResponseNoContentAsync(req, HttpStatusCode.UnprocessableEntity, responseMessage, ex);

            //var responsePayload1 =
            //    ApiResponse.Failed(HttpStatusCode.UnprocessableEntity,
            //        ex.ToString(),
            //        responseMessage);

            //var response1 = req.CreateResponse(HttpStatusCode.UnprocessableEntity);
            //response1.Headers.Add("Content-Type", "application/json; charset=utf-8");
            //await response1.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(responsePayload1));

            //return response1;
        }

        //var response = req.CreateResponse(HttpStatusCode.OK);
        //await response.WriteAsJsonAsync(book);
        //return response;

        responseMessage = $"Function triggered successfully and book retrieved.";
        return await ResponseFactory.CreateSuccessResponseAsync<Book>(req, responseMessage, book);

        //ApiResponse<Book> responsePayload;

        //if (book != null)
        //{
        //    responsePayload = ApiResponse<Book>.Success(book, responseMessage);
        //}
        //else
        //{
        //    responsePayload = ApiResponse<Book>.SuccessNoContent(responseMessage);
        //}

        //var response = req.CreateResponse(HttpStatusCode.OK);
        //response.Headers.Add("Content-Type", "application/json; charset=utf-8");

        //await response.WriteStringAsync(System.Text.Json.JsonSerializer.Serialize(responsePayload));

        //return response;
    }
}
