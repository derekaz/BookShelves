
using Serilog;

namespace BookShelves.Maui
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Log.Information("CreateWindow-Start");
            var window = new Window(new MainPage()) { Title = "BookShelves" };
#if WINDOWS
            window.Height = 806;
#endif
            Log.Information("CreateWindow-End");
            return window;
        }
    }
}
