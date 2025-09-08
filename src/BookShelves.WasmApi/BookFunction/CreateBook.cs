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
using System.Transactions;

namespace BookShelves.WasmApi.BookFunction;

public class CreateBook
{
    private readonly ILogger<CreateBook> _logger;
    private readonly BookRepository _bookRepository;
    private readonly UniqueIdRepository _uniqueIdRepository;

    public CreateBook(ILogger<CreateBook> logger, BookRepository bookRepository, UniqueIdRepository uniqueIdRepository)
    {
        _logger = logger;
        _bookRepository = bookRepository;
        _uniqueIdRepository = uniqueIdRepository;
    }

    [Function("CreateBook")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = $"books/new")] HttpRequestData req)
    {
        _logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(Run)}");
        string? title = req.FunctionContext.BindingContext.BindingData["title"]!.ToString();
        string? author = req.FunctionContext.BindingContext.BindingData["author"]!.ToString();

        string? requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic? data = JsonConvert.DeserializeObject(requestBody);
        title ??= data?.title;
        author ??= data?.author;

        if (string.IsNullOrEmpty(title))
        {
            _logger.LogInformation($"Unable to create book with no title.");
            return req.CreateResponse(HttpStatusCode.UnprocessableEntity);
        }

        Book? newBook;

        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            long? newUniqueId = null;
            try
            {
                var temp = await _uniqueIdRepository.GetAsync(Book.BOOKS_UNIQUEID_RECORD_ID); // ?? throw new ApplicationException("Unable to get unique id");
                if (temp == null)
                {
                    temp = new UniqueId() { 
                        Id = Book.BOOKS_UNIQUEID_RECORD_ID,
                        UniqueIdValue = 0 
                    };
                }
                temp.UniqueIdValue++;

                await _uniqueIdRepository.UpdateAsync(Book.BOOKS_UNIQUEID_RECORD_ID, temp);
                newUniqueId = temp.UniqueIdValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unable to get id for new book: {data}");
                return req.CreateResponse(HttpStatusCode.UnprocessableEntity);
            }

            newBook = new()
            {
                Id = newUniqueId.ToString(),
                Title = title ?? string.Empty,
                Author = author ?? string.Empty
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
}
