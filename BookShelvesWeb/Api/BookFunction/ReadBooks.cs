using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using BlazorApp.Api.DataAccess;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;

namespace BlazorApp.Api.BookFunction
{
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

            var searchterm = req.FunctionContext.BindingContext.BindingData["title"].ToString();
            if (string.IsNullOrWhiteSpace(searchterm))
            {
                return req.CreateResponse(HttpStatusCode.NotFound);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(await booksData.GetMultipleAsync($"SELECT * FROM items i WHERE CONTAINS '{searchterm}'"));
            return response;
        }
    }
}
