using BookShelves.Maui.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookShelves.Maui.Data.Services;

public class BookShelvesDbContext : DbContext
{
    private readonly int LATEST_DATABASE_VERSION = 3;
    private readonly ILogger _logger;

    public DbSet<LocalBook> Books { get; set; }

    public BookShelvesDbContext(DbContextOptions<BookShelvesDbContext> options, ILogger<BookShelvesDbContext> logger) : base(options)
    {
        _logger = logger;

        var connectionString = Database.GetDbConnection().ConnectionString;
        //if (options.Extensions.OfType<SqliteOptionsExtension>().FirstOrDefault() is SqliteOptionsExtension sqliteOptions)
        //{

        //}

        _logger.LogInformation($"BookShelvesContext-Constructor; dbPath: {connectionString}");
        //Database.EnsureCreated();
        UpdateDatabaseIfRequired();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _logger.LogInformation("BookShelvesContext-OnModelCreating");
        modelBuilder.Entity<LocalBook>().HasData(new LocalBook() { Id = 1, Title = "Book 1", Author = "Author 1", LastUpdateTime = DateTime.UtcNow, Revision = 1 });
        //base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        _logger.LogInformation("BookShelvesContext-OnConfiguring");
        if (!optionsBuilder.IsConfigured)
        {
            _logger.LogInformation("BookShelvesContext-OnConfiguring-NotConfigured");
            //optionsBuilder.UseSqlite($"Data Source={DbPath}");
            //var sqliteConnectionInitializer = new CreateOrMigrateDatabaseInitializer<BookShelvesDbContext>();
            //Database.SetInitializer(sqliteConnectionInitializer);
            SQLitePCL.Batteries_V2.Init();
        }
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
        try
        {
            if (Database.CanConnect())
            {
                _logger.LogInformation($"ConnectionString={Database.GetDbConnection().ConnectionString}");
                long currentDbVersion = GetUserVersion();

                _logger.LogInformation($"currentDbVersion: {currentDbVersion}");

                //IEnumerable<string> tables = Database.SqlQueryRaw<string>($"SELECT name FROM sqlite_master WHERE type = 'table';")
                //    .AsEnumerable();

                long hasTables = Database.SqlQueryRaw<long>($"SELECT COUNT(*) AS TableCount FROM sqlite_master WHERE type = 'table' AND name = 'books';")
                    .AsEnumerable().FirstOrDefault();

                _logger.LogDebug($"hasTables: {hasTables}");

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
                                currentDbVersion = UpgradeToOne();
                                break;
                            case 2:
                                currentDbVersion = UpgradeToTwo();
                                break;
                            case 3:
                                currentDbVersion = UpgradeToThree();
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
                    SetUserVersion(LATEST_DATABASE_VERSION);
                }

            }
            else
            {
                Database.EnsureCreated();
                SetUserVersion(LATEST_DATABASE_VERSION);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BookShelvesContext:UpdateDatabaseIfRequired-Exception");
            throw;
        }
    }

    private int GetUserVersion()
    {
        string script = "PRAGMA user_version;";
        try
        {
            long version = Database.SqlQueryRaw<long>(script).AsEnumerable().FirstOrDefault();
            _logger.LogInformation($"User Version: {version}");
            return Convert.ToInt32(version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BookShelvesContext:GetUserVersion-Exception");
            throw;
        }
    }

    private void SetUserVersion(long version)
    {
        FormattableString script = $"PRAGMA user_version={version};";
        try
        {
            int rows_affected = Database.ExecuteSqlRaw(script.ToString());
            _logger.LogInformation($"Rows Affected: {rows_affected}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BookShelvesContext:SetUserVersion-Exception");
            throw;
        }
    }   

    private int UpgradeToOne()
    {
        int VERSION = 1;

        FormattableString[] stepScripts = { $"ALTER TABLE Books ADD COLUMN LastUpdateTime DATETIME;", $"ALTER TABLE Books ADD COLUMN Revision INT;" };
        try
        {
            foreach (var stepScript in stepScripts)
            {
                int rows_affected = ExecuteUpdateStep(stepScript, true);
                _logger.LogInformation($"Rows Affected: {rows_affected}");
            }

            SetUserVersion(VERSION);
            var newVersion = GetUserVersion();

            if (newVersion != VERSION)
            {
                throw new ApplicationException("Unable to upgrade DB Version");
            }

            return newVersion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BookShelvesContext:UpgradeToOne-Exception");
            throw;
        }
    }

    private int UpgradeToTwo()
    {
        int VERSION = 2;

        FormattableString[] stepScripts = { $"ALTER TABLE Books ADD COLUMN UpdateType TEXT;" };
        try
        {
            foreach (var stepScript in stepScripts)
            {
                int rows_affected = ExecuteUpdateStep(stepScript, true);
                _logger.LogInformation($"Rows Affected: {rows_affected}");
            }

            SetUserVersion(VERSION);
            var newVersion = GetUserVersion();

            if (newVersion != VERSION)
            {
                throw new ApplicationException("Unable to upgrade DB Version");
            }

            return newVersion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BookShelvesContext:UpgradeToTwo-Exception");
            throw;
        }
    }

    private int UpgradeToThree()
    {
        int VERSION = 3;

        FormattableString[] stepScripts = { $"ALTER TABLE Books ADD COLUMN ServerId INT;" };
        try
        {
            foreach (var stepScript in stepScripts)
            {
                int rows_affected = ExecuteUpdateStep(stepScript, true);
                _logger.LogInformation($"Rows Affected: {rows_affected}");
            }

            SetUserVersion(VERSION);
            var newVersion = GetUserVersion();

            if (newVersion != VERSION)
            {
                throw new ApplicationException("Unable to upgrade DB Version");
            }

            return newVersion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BookShelvesContext:UpgradeToThree-Exception");
            throw;
        }
    }

    private int ExecuteUpdateStep(FormattableString? script, bool swallowDuplicateColumnErrors = false)
    {
        if (script == null)
        {
            return 0;
        }

        var itemString = script.ToString();

        if (String.IsNullOrEmpty(itemString))
        {
            return 0;
        }

        try
        {
            int rows_affected = Database.ExecuteSqlRaw(itemString);
            _logger.LogInformation($"Rows Affected: {rows_affected}");
            return rows_affected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BookShelvesContext:ExecuteUpdateStep-Exception");

            if (swallowDuplicateColumnErrors && ex.Message.Contains("duplicate column name"))
            {
                return 0;
            }

            throw;
        }
    }
}
