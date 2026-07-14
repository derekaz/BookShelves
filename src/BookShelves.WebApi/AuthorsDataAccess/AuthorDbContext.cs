using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics.CodeAnalysis;

namespace BookShelves.WebApi.AuthorsDataAccess;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AuthorItem> AuthorItems => Set<AuthorItem>();

    //public DbSet<TodoList> TodoLists => Set<TodoList>();

    public async Task InitializeDatabaseAsync()
    {
        _ = await Database.EnsureCreatedAsync();

        const string datasyncTrigger = @"
            CREATE OR ALTER TRIGGER [dbo].[{0}_datasync] ON [dbo].[{0}] AFTER INSERT, UPDATE AS
            BEGIN
                SET NOCOUNT ON;
                UPDATE
                    [dbo].[{0}]
                SET
                    [UpdatedAt] = SYSUTCDATETIME()
                WHERE
                    [Id] IN (SELECT [Id] FROM INSERTED);
            END
        "
        ;

        // Install the above trigger to set the UpdatedAt field automatically on insert or update.
        foreach (IEntityType table in Model.GetEntityTypes())
        {
            string sql = string.Format(datasyncTrigger, table.GetTableName());
            _ = await Database.ExecuteSqlRawAsync(sql);
        }
    }

    [SuppressMessage("Style", "IDE0058:Expression value is never used", Justification = "Model builder ignores return value.")]
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Tells EF Core that the AuthorItem entity has a trigger.
        modelBuilder.Entity<AuthorItem>()
            .ToTable(tb => tb.HasTrigger("AuthorItem_datasync"));

        // Tells EF Core that the TodoList entity has a trigger.
        //modelBuilder.Entity<TodoList>()
        //    .ToTable(tb => tb.HasTrigger("TodoList_datasync"));

        base.OnModelCreating(modelBuilder);
    }
}