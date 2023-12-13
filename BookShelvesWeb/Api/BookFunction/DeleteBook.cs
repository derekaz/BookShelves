using BlazorApp.Api.DataAccess;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Net;

namespace BlazorApp.Api.BookFunction
{
    public class DeleteBook
    {
        private readonly ILogger<DeleteBook> logger;
        private readonly BookRepository booksData;

        public DeleteBook(ILogger<DeleteBook> logger, BookRepository booksData)
        {
            this.logger = logger;
            this.booksData = booksData;
        }

        [Function("DeleteBook1")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "book/{id}")] HttpRequestData req, string id
            )
        {
            logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(Run)} with id:{id}");

            try
            {
                await booksData.DeleteAsync(id);
            }
            catch (Exception ex) 
            {
                logger.LogError(ex, $"Unable to delete book: {id}");
                return req.CreateResponse(HttpStatusCode.UnprocessableEntity);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
