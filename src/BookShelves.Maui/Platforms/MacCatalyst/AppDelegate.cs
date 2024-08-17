using BookShelves.Maui.Data;
using Foundation;

namespace BookShelves.Maui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp()
        {
            BookShelves.Maui.Data.SqliteProvider.RegisterSqliteProvider();
            return MauiProgram.CreateMauiApp();
        }
    }
}
