using BookShelves.Shared.Data.Models;

namespace BookShelves.Shared.Data.Interfaces;

public interface IWeatherForecasterService
{
    Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync();
}
