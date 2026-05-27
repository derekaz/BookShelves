using BookShelves.Shared.Data.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;

namespace BookShelves.WasmApi;

public class WeatherForecastFunction
{
    private readonly ILogger _logger;

    public WeatherForecastFunction(ILogger<WeatherForecastFunction> logger)
    {
        _logger = logger;
    }

    private string GetSummary(int temp)
    {
        var summary = "Mild";

        if (temp >= 32)
        {
            summary = "Hot";
        }
        else if (temp <= 16 && temp > 0)
        {
            summary = "Cold";
        }
        else if (temp <= 0)
        {
            summary = "Freezing!";
        }

        return summary;
    }

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [Function("WeatherForecast")]
    public HttpResponseData Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req)
    {
        var randomNumber = new Random();
        var temp = 0;

        var result = Enumerable.Range(1, 5).Select(index => new BookShelves.Shared.Data.Models.WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        }).Cast<IWeatherForecast>().ToArray();

        //var result = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //{
        //    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //    TemperatureC = temp = randomNumber.Next(-20, 55),
        //    Summary = GetSummary(temp)
        //}).ToArray();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteAsJsonAsync(result);

        return response;
    }
}
