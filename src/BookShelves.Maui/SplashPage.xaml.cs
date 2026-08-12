using Serilog;

namespace BookShelves.Maui;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        try
        {
            Log.Information("SplashPage-InitializeComponent-Start");
            InitializeComponent();
            Log.Information("SplashPage-InitializeComponent-End");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "SplashPage initialization failed");
            throw;
        }
    }
}
