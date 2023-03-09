using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using System.Linq;
using BlazorApp.Api.DataAccess;
using Microsoft.AspNetCore.Mvc.Formatters.Internal;

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

        [FunctionName("DeleteBook1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "book/{id}")] HttpRequest req, string id
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
                return new UnprocessableEntityResult();
            }

            return new OkResult();
        }
    }
}
