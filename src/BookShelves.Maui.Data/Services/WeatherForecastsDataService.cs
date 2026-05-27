using BookShelves.Shared.Data.Interfaces;

namespace BookShelves.Maui.Data.Services
{
    public class WeatherForecastsDataService : IWeatherForecastService
    {
        public async Task<IEnumerable<IWeatherForecast>> GetWeatherForecastsAsync()
        {
            // HttpContext.VerifyUserHasAnyAcceptedScope("Weather.Get");
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            });
        }

        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];
    }
}