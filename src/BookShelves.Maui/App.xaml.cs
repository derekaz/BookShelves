
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
            Log.Information("M15-CreateWindow-Start");
            var window = new Window(new MainPage()) { Title = "BookShelves" };
            Log.Information("M16-CreateWindow-End");
            return window;
        }
    }
}
