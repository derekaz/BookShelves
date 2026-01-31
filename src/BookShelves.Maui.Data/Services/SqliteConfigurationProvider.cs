using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BookShelves.Maui.Data.Services;

public class SqliteConfigurationProvider : ConfigurationProvider
{
    private readonly string _connectionString;
    private readonly ILoggerFactory _loggerFactory;

    public SqliteConfigurationProvider(string connectionString, ILoggerFactory loggerFactory)
    {
        _connectionString = connectionString;
        _loggerFactory = loggerFactory;
    }

    public override void Load()
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<BookShelvesDbContext>();
            optionsBuilder.UseSqlite(_connectionString);

            var settingsLogger = _loggerFactory.CreateLogger<BookShelvesDbContext>();

            using var context = new BookShelvesDbContext(optionsBuilder.Options, settingsLogger);
            Data = context.ConfigurationSettings.Any()
                ? context.ConfigurationSettings.ToDictionary(
                    c => c.Key,
                    c => c.Value,
                    StringComparer.OrdinalIgnoreCase
                )
                : new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            // Log the error and proceed with empty data
            Console.WriteLine($"Error loading configuration from SQLite: {ex.Message}");
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public void Save()
    {
        PersistChanges(Data);
    }

    private void PersistChanges(IDictionary<string, string?> newSettings)
    {
        // Logic to save newSettings to your custom source (e.g., database, custom file)
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<BookShelvesDbContext>();
            optionsBuilder.UseSqlite(_connectionString);

            var settingsLogger = _loggerFactory.CreateLogger<BookShelvesDbContext>();

            using var context = new BookShelvesDbContext(optionsBuilder.Options, settingsLogger);

            foreach (var existingSetting in context.ConfigurationSettings)
            {
                if (!newSettings.ContainsKey(existingSetting.Key))
                {
                    context.ConfigurationSettings.Remove(existingSetting);
                }
            }

            foreach (var newSetting in newSettings)
            {
                var existingSetting = context.ConfigurationSettings
                    .FirstOrDefault(s => s.Key.ToLower().Equals(newSetting.Key.ToLower()));

                if (existingSetting != null)
                {
                    if (newSetting.Value == null)
                    {
                        context.ConfigurationSettings.Remove(existingSetting);
                        continue;
                    }

                    if (newSetting.Value != existingSetting.Value)
                    {
                        // update only if the value is different

                        existingSetting.Value = newSetting.Value;
                        existingSetting.LastUpdateTime = DateTime.UtcNow;
                        context.ConfigurationSettings.Update(existingSetting);
                    }
                }
                else
                {
                    context.ConfigurationSettings.Add(new Models.ConfigurationSetting
                    {
                        Key = newSetting.Key,
                        Value = newSetting.Value,
                        LastUpdateTime = DateTime.UtcNow
                    });
                }
            }

            context.SaveChanges();
        }
        catch (Exception ex)
        {
            // Log the error and proceed with empty data
            Console.WriteLine($"Error loading configuration from SQLite: {ex.Message}");
        }

        // After saving, you might want to reload the configuration to reflect changes
        Load();
    }
}