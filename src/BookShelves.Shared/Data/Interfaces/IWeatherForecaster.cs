using BookShelves.Shared.Data.Models;

namespace BookShelves.Shared.Data.Interfaces;

public interface IWeatherForecaster
{
    Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync();
}
