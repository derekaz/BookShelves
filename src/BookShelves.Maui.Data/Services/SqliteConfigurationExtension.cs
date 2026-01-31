using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BookShelves.Maui.Data.Services;

public static class SqliteConfigurationExtensions
{
    public static IConfigurationBuilder AddSqliteConfiguration(this IConfigurationBuilder builder, string connectionString, ILoggerFactory loggerFactory)
    {
        return builder.Add(new SqliteConfigurationSource(connectionString, loggerFactory));
    }
}