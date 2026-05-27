namespace BookShelves.Shared.Data.Interfaces;

public interface IWeatherForecaster
{
    Task<IEnumerable<IWeatherForecast>> GetWeatherForecastAsync();
}
