// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BookShelves.Maui.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // Use the native event handler directly on this instance
            this.UnhandledException += (sender, e) =>
            {
                var errorMessage = e.Message;
                var exceptionDetails = e.Exception?.ToString() ?? "No inner exception details.";

                // Write to a local text file on your Desktop since you cannot see the IDE output during a packaged run
                try
                {
                    string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
                    string logPath = System.IO.Path.Combine(desktopPath, "BookShelves_Crash_Log.txt");
                    System.IO.File.WriteAllText(logPath, $"Error: {errorMessage}\n\nDetails:\n{exceptionDetails}");
                }
                catch
                {
                    // Fallback if desktop access is restricted by package permissions
                }
            };
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }

}
