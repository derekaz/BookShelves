using BookShelves.Maui.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BookShelves.Maui.Data.Services;

public class BookShelvesContext : DbContext
{
    private readonly int LATEST_DATABASE_VERSION = 1;

    public DbSet<Book> Books { get; set; }

    //public string DbPath { get; private set; }

    public BookShelvesContext(DbContextOptions<BookShelvesContext> options) : base(options)
    {
        //DbPath = dbPath;
        Database.EnsureCreated();
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
        long currentDbVersion = Database.SqlQueryRaw<long>("PRAGMA user_version")
                                .AsEnumerable().FirstOrDefault();

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
        // Finally, set the db version to latest
        Database.ExecuteSqlRaw($"PRAGMA user_version={LATEST_DATABASE_VERSION}");
    }

    private void UpgradeToOne()
    {
        FormattableString script = $"ALTER TABLE Books ADD COLUMN LastUpdateTime DATETIME; ALTER TABLE Books ADD COLUMN Revision INT;";
        int rows_affected = Database.ExecuteSql(script);
        Debug.WriteLine(rows_affected);
    }

}
