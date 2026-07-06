using BookShelves.Shared.Data.Models;
using BookShelves.WasmApi.Utilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BookShelves.WasmApi.WeatherFunction;

public class GetWeatherForecasts
{
    private readonly ILogger<GetWeatherForecasts> _logger;

    public GetWeatherForecasts(ILogger<GetWeatherForecasts> logger)
    {
        _logger = logger;
    }

    private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

    [Function("GetWeatherForecasts1")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "weatherforecasts")] HttpRequestData req
    )
    {
        _logger.LogInformation($"C# HTTP trigger function processed a request. Function name: {nameof(Run)}");

        IEnumerable<WeatherForecast> forecasts;
        string responseMessage;

        try
        {
            forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)],
                LastUpdated = DateTime.Now,
                Source = "Server API"
            }); //.Cast<IWeatherForecast>();
        }
        catch (Exception ex)
        {
            responseMessage = $"Unable to retrieve weather forecasts";
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

        responseMessage = $"Function triggered successfully and forecasts retrieved.";
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(forecasts);
        return response; // await ResponseFactory.CreateSuccessResponseAsync<IEnumerable<WeatherForecast>>(req, responseMessage, forecasts);

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
