using BookShelves.Maui.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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
        //Database.EnsureCreated();
        UpdateDatabaseIfRequired();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>().HasData(new Book() { Id = 1, Title = "Book 1", Author = "Author 1", LastUpdateTime = DateTime.UtcNow, Revision = 1 });
        //base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //optionsBuilder.UseSqlite($"Data Source={DbPath}");
        //var sqliteConnectionInitializer = new CreateOrMigrateDatabaseInitializer<BookShelvesContext>();
        //Database.SetInitializer(sqliteConnectionInitializer);
        SQLitePCL.Batteries_V2.Init();
        base.OnConfiguring(optionsBuilder);
    }

    //private class CreateOrMigrateDatabaseInitializer<TContext> : CreateDatabaseIfNotExists<TContext>, IDatabaseInitializer<TContext> where TContext : DbContext
    //{
    //    void IDatabaseInitializer<TContext>.InitializeDatabase(TContext context)
    //    {
    //        base.InitializeDatabase(context);

    //        if (context.Database.Exists())
    //        {
    //            Migrate(context);
    //        }
    //    }

    //    private void Migrate(DbContext context)
    //    {
    //        int version = context.Database.SqlQuery<int>($"PRAGMA user_version").First();
    //        int numTables = context.Database.SqlQuery<int>($"SELECT COUNT(*) FROM sqlite_master AS TABLES WHERE TYPE = 'table'").First();

    //        if (numTables == 0)
    //        {
    //            context.Database.ExecuteSqlRaw(Constants.CreateBookTableStatement);
    //        }

    //        //foreach (var migrationFile in Directory.GetFiles("migrations/", "*.sql"))
    //        //{
    //        //    if (!int.TryParse(Path.GetFileName(migrationFile).Split('.').First(), out int sqlVersion))
    //        //    {
    //        //        continue;
    //        //    }

    //        //    if (sqlVersion <= version)
    //        //    {
    //        //        continue;
    //        //    }

    //        //    var migrationScript = File.ReadAllText(migrationFile);
    //        //    context.Database.ExecuteSqlRaw(migrationScript);
    //        //}
    //    }
    //}

    private void UpdateDatabaseIfRequired()
    {
        //Database.GetService<IDatabaseCreator>().CanConnect();
        //Database.CanConnect();
        if ((Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator).Exists())
        {
        }


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
