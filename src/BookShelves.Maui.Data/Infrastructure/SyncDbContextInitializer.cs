using BookShelves.Maui.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookShelves.Maui.Data.Infrastructure;

/// <summary>
/// Use this class to initialize the database.  In this sample, we just create
/// the database. However, you may want to use migrations.
/// </summary>
/// <param name="context">The context for the database.</param >
public class SyncDbContextInitializer(SyncDbContext context, ILogger<SyncDbContextInitializer> logger) : IDbInitializer
{
    /// <inheritdoc />
    public void Initialize()
    {
        try
        {
            logger.LogInformation("Starting synchronous database migration optimization...");
            context.Database.Migrate();
            logger.LogInformation("Database migration optimized successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Synchronous database migration crashed on application boot.");
            throw;
        }

        // _ = context.Database.EnsureCreated();
        // Task.Run(async () => await context.SynchronizeAsync());
    }

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Starting asynchronous database migration pipeline...");

            // This runs the migration and updates your SQLite file
            await context.Database.MigrateAsync(cancellationToken);

            logger.LogInformation("Asynchronous database migration completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Asynchronous database migration crashed on application boot.");
            throw;
        }

        // return context.Database.EnsureCreatedAsync(cancellationToken);
    }
}