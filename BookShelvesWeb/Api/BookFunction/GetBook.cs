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
    public class GetBook
    {
        private readonly ILogger<GetBook> logger;
        private readonly BookRepository booksData;

        public GetBook(ILogger<GetBook> logger, BookRepository booksData)
        {
            this.logger = logger;
            this.booksData = booksData;
        }

        [FunctionName("GetBook1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "book/{id}")] HttpRequest req, string id
            )
        {
            logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(Run)} with id:{id}");

            return new OkObjectResult(await booksData.GetAsync(id));
        }
    }
}
