using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace BookShelves.WebApi.Controllers
{
    [Authorize()]
    [ApiController]
    [Route("[controller]")]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet(Name = "GetWeatherForecast")]
        [RequiredScopeOrAppPermission(AcceptedScope = new[] { "Weather.Get" })]
        public async Task<IResult> Get()
        {
            // HttpContext.VerifyUserHasAnyAcceptedScope("Weather.Get");
            var data = Enumerable.Range(1, 5).Select(index => new BookShelves.Shared.Data.Models.WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)],
                LastUpdated = DateTime.Now,
                Source = "Web API"
            });

            return TypedResults.Ok(data);
        }
    }
}
