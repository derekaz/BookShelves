using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Data.Models;

namespace BookShelves.Maui.Data.Services
{
    public class WeatherForecastsDataService : IWeatherForecaster
    {
        //public async Task<IEnumerable<IWeatherForecast>> GetWeatherForecastsAsync()
        //{
        //    // HttpContext.VerifyUserHasAnyAcceptedScope("Weather.Get");
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    });
        //}

        public Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
        {
            // HttpContext.VerifyUserHasAnyAcceptedScope("Weather.Get");

            return Task.FromResult(Enumerable.Range(1, 5).Select(index => new BookShelves.Shared.Data.Models.WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }));
            // .Cast<IWeatherForecast>())

            //return Enumerable.Range(1, 5).Select(index => new BookShelves.Shared.Data.Models.WeatherForecast
            //{
            //    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            //    TemperatureC = Random.Shared.Next(-20, 55),
            //    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            //});
        }

        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];
    }
}