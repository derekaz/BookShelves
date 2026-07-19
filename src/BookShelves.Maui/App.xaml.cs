
using BookShelves.Maui.Data.SyncTest;
using Microsoft.EntityFrameworkCore;

namespace BookShelves.Maui
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Automatically provision the database when the app starts
            Task.Run(async () =>
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<SyncDbContext>();
                await dbContext.Database.MigrateAsync();
            });
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "BookShelves" };
        }
    }
}
