using BookShelves.Maui.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using static SQLite.SQLite3;

namespace BookShelves.Maui.Data.Services;

public class BookShelvesContext : DbContext
{
    private readonly int LATEST_DATABASE_VERSION = 1;
    private readonly ILogger _logger;

    public DbSet<Book> Books { get; set; }

    //public string DbPath { get; private set; }

    public BookShelvesContext(DbContextOptions<BookShelvesContext> options, ILogger<BookShelvesContext> logger) : base(options)
    {
        _logger = logger;
        //DbPath = dbPath;
        //Database.EnsureCreated();
        UpdateDatabaseIfRequired();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.UseSqlite($"Data Source={DbPath}");
        SQLitePCL.Batteries_V2.Init();
        base.OnConfiguring(optionsBuilder);
    }

    private void UpdateDatabaseIfRequired()
    {
        try
        {
            if (Database.CanConnect())
            {
                long currentDbVersion = Database.SqlQueryRaw<long>("PRAGMA user_version")
                    .AsEnumerable().FirstOrDefault();

                //IEnumerable<string> tables = Database.SqlQueryRaw<string>($"SELECT name FROM sqlite_master WHERE type = 'table';")
                //    .AsEnumerable();

                long hasTables = Database.SqlQueryRaw<long>($"SELECT COUNT(*) AS TableCount FROM sqlite_master WHERE type = 'table' AND name = 'books';")
                    .AsEnumerable().FirstOrDefault();

                if (currentDbVersion == 0 && hasTables == 0)
                {
                    Database.EnsureCreated();
                }
                else
                {
                    if (LATEST_DATABASE_VERSION > currentDbVersion)
                    {
                        var upgradeToDbVersion = currentDbVersion + 1;
                        switch (upgradeToDbVersion)
                        {
                            case 1:
                                UpgradeToOne();
                                //    goto case 2;
                                //case 2:
                                //    UpgradeToTwo();
                                break;
                            default:
                                Database.EnsureCreatedAsync();
                                break;
                        }

                    }
                }

                if (LATEST_DATABASE_VERSION != currentDbVersion)
                {
                    // Finally, set the db version to latest
                    Database.ExecuteSqlRaw($"PRAGMA user_version={LATEST_DATABASE_VERSION}");
                }

            }
            else
            {
                Database.EnsureCreated();
                Database.ExecuteSqlRaw($"PRAGMA user_version={LATEST_DATABASE_VERSION}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BookShelvesContext:UpdateDatabaseIfRequired-Exception");
            throw;
        }
    }

    private void UpgradeToOne()
    {
        FormattableString script = $"ALTER TABLE Books ADD COLUMN LastUpdateTime DATETIME; ALTER TABLE Books ADD COLUMN Revision INT;";
        int rows_affected = Database.ExecuteSql(script);
        _logger.LogInformation($"{rows_affected}");
    }

}
