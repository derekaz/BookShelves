
namespace BookShelves.Maui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new MainPage();
	}

    protected override Window CreateWindow(IActivationState? activationState)
    {
		var window = base.CreateWindow(activationState);
		window.MinimumWidth = 300;
		window.MinimumHeight = 400;
        return window;
    }
}
