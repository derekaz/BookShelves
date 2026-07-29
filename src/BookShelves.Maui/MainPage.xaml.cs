using Serilog;

namespace BookShelves.Maui
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            try
            {
                Log.Information("M17-MainPage-InitializeComponent-Start");
                InitializeComponent();
                Log.Information("M18-MainPage-InitializeComponent-End");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "MainPage initialization failed");
                throw;
            }
        }
    }
}
