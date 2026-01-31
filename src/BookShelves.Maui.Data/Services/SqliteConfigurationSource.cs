using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BookShelves.Maui.Data.Services;

public class SqliteConfigurationSource(string connectionString, ILoggerFactory loggerFactory) : IConfigurationSource
{
    private readonly string _connectionString = connectionString;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new SqliteConfigurationProvider(_connectionString, _loggerFactory);
    }
}
