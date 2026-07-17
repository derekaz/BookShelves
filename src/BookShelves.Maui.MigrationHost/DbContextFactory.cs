using BookShelves.Maui.Data.SyncTest;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BookShelves.Maui.MigrationHost
{
    internal class DbContextFactory : IDesignTimeDbContextFactory<SyncDbContext>
    {
        public SyncDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<SyncDbContext>();

            // Configure a dummy location provider for design-time migrations. Change to your real provider/connection string.
            builder.UseSqlite("Data Source=BookShelves.Migrations.db");

            // Try to construct AuthorDbContext with the options. If AuthorDbContext has a (DbContextOptions<AuthorDbContext>) ctor,
            // this will call it. If not, provide the actual constructor signature here or share the AuthorDbContext constructors.
            var ctx = Activator.CreateInstance(typeof(SyncDbContext), builder.Options) as SyncDbContext;
            if (ctx == null)
                throw new InvalidOperationException("Unable to create an AuthorDbContext. Ensure it has a constructor accepting DbContextOptions<AuthorDbContext>.");

            return ctx;
        }
    }
}
