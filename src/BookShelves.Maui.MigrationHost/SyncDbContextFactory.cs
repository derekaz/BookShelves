using BookShelves.Maui.Data.Infrastructure;
using BookShelves.Maui.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Logging.Abstractions;

namespace BookShelves.Maui.MigrationHost;

public class SyncDbContextFactory : IDesignTimeDbContextFactory<SyncDbContext>
{
    public SyncDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SyncDbContext>();

        // Match the provider you are using on the client (usually SQLite)
        // Target your Migrations project assembly so EF knows where to generate files
        optionsBuilder.UseSqlite(
            "Data Source=design_time.db",
            options => options.MigrationsAssembly("BookShelves.Maui.Data")
        );

        // Pass Null/Mock dependencies safely for design-time use
        return new SyncDbContext(
            optionsBuilder.Options,
            NullLogger<SyncDbContext>.Instance,
            new DesignTimeSyncApiClient()
        );
    }
}

internal class DesignTimeSyncApiClient : ISyncApiClient
{
    public HttpClient HttpClient { get; } = new();
}
