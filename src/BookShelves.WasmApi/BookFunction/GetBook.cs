using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BlazorApp.Api.DataAccess;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;

namespace BlazorApp.Api.BookFunction
{
    public class GetBook
    {
        private readonly ILogger<GetBook> logger;
        private readonly BookRepository booksData;

        public GetBook(ILogger<GetBook> logger, BookRepository booksData)
        {
            this.logger = logger;
            this.booksData = booksData;
        }

        [Function("GetBook1")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "book/{id}")] HttpRequestData req, 
            string id
        )
        {
            logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(Run)} with id:{id}");

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(await booksData.GetAsync(id));
            return response;
        }
    }
}
