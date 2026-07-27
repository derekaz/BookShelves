
using BookShelves.Maui.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BookShelves.Maui
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Automatically provision the database when the app starts.
                // Exceptions are caught and written to the crash log so that a migration
                // failure does not become a silent unobserved-task crash on iOS.
                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = serviceProvider.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<SyncDbContext>();
                        await dbContext.Database.MigrateAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CRITICAL] Database migration failed: {ex}");
                        try
                        {
                            var crashPath = Helpers.FileAccessHelper.GetLogFilePath("db-migration-crash.log");
                            System.IO.File.AppendAllText(crashPath,
                                $"=== DB Migration Failure ({DateTime.UtcNow:O}) ===\n{ex}\n\n");
                        }
                        catch { /* best-effort only */ }
                    }
                });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "BookShelves" };
        }
    }
}
