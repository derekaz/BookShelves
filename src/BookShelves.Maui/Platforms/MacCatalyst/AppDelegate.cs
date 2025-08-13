using BookShelves.Maui.Data.Extensions;
using Foundation;

namespace BookShelves.Maui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp()
        {
            SqliteProviderExtension.RegisterSqliteProvider();
            return MauiProgram.CreateMauiApp();
        }
    }
}
