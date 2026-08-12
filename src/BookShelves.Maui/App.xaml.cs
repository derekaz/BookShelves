
using BookShelves.Maui.Services;
using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BookShelves.Maui
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        private Window? _window;

        public App(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Log.Information("CreateWindow-Start");

            // Show splash screen while initializing
            var splashPage = new SplashPage();
            _window = new Window(splashPage) { Title = "BookShelves" };

#if WINDOWS
            _window.Height = 806;
#endif

            // Initialize authentication state from MSAL cache on app startup
            // This allows users to remain logged in across app restarts
            InitializeAuthenticationAsync();

            Log.Information("CreateWindow-End");
            return _window;
        }

        /// <summary>
        /// Initializes authentication state asynchronously.
        /// This runs in the background to restore the user session from MSAL cache if available.
        /// Once complete, transitions from splash screen to main application.
        /// </summary>
        private async void InitializeAuthenticationAsync()
        {
            try
            {
                Log.Information("App: Initializing authentication state");

                var authService = _serviceProvider.GetService<IAuthService>();
                if (authService is not null)
                {
                    await authService.InitializeAsync();
                    Log.Information("App: Authentication state initialized");
                }
                else
                {
                    Log.Warning("App: IAuthService is not registered; skipping startup auth initialization");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "App: Error during authentication initialization");
                // Continue anyway - user can log in manually if needed
            }
            finally
            {
                // Transition from splash screen to main application
                await TransitionToMainPageAsync();
            }
        }

        /// <summary>
        /// Transitions from the splash screen to the main application page.
        /// </summary>
        private async Task TransitionToMainPageAsync()
        {
            try
            {
                Log.Information("App: Transitioning to main page");

                if (_window != null)
                {
                    // Fade out the splash screen and transition to MainPage
                    var mainPage = new MainPage();

                    // Animate the transition - fade out splash screen
                    if (_window.Page != null)
                    {
                        await _window.Page.FadeToAsync(0, 300, Easing.CubicInOut);
                    }

                    // Switch to main page
                    _window.Page = mainPage;

                    // Fade in the main page
                    await mainPage.FadeToAsync(1, 300, Easing.CubicInOut);
                }

                Log.Information("App: Main page displayed");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "App: Error transitioning to main page");
                // Fallback: just set the page without animation
                if (_window != null)
                {
                    _window.Page = new MainPage();
                }
            }
        }
    }
}
