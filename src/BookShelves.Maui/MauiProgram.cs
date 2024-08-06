using BookShelves.Maui.Data;
using BookShelves.Maui.Helpers;
using BookShelves.Maui.Services;
using BookShelves.Shared;
using BookShelves.Shared.DataInterfaces;
using CommunityToolkit.Maui;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Maui.LifecycleEvents;
using System.Reflection;

namespace BookShelves.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif


        builder.ConfigureLifecycleEvents(events =>
        {
#if ANDROID
                events.AddAndroid(platform =>
                {
                    platform.OnActivityResult((activity, rc, result, data) =>
                    {
                        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(rc, result, data);
                    });
                });
#endif
        });

#if ANDROID
        builder.Services.AddSingleton<IWindowService, Platforms.Android.WindowService>();
#elif IOS

#elif MACCATALYST

#elif WINDOWS
        builder.Services.AddSingleton<IWindowService, Platforms.Windows.WindowService>();
#endif

        builder.Services.AddOptions();
        builder.Services.AddAuthorizationCore();
        builder.Services.AddCascadingAuthenticationState();

        //var baseUrl = string.Join("/",
        //    builder.Configuration.GetSection("MicrosoftGraph")["BaseUrl"],
        //    builder.Configuration.GetSection("MicrosoftGraph")["Version"]);
        //var scopes = builder.Configuration.GetSection("MicrosoftGraph:Scopes")
        //    .Get<List<string>>();
        //builder.Services.AddGraphClient(baseUrl, scopes);


        var assembly = Assembly.GetExecutingAssembly();
        var appName = assembly.GetName().Name;

        using var appSettingsStream = assembly.GetManifestResourceStream($"{appName}.appSettings.json");
        using var appSettingsDevStream = assembly.GetManifestResourceStream($"{appName}.appSettings.Development.json");

        var configBuilder = new ConfigurationBuilder();

        // Add appSettings.json to configuration
        if (appSettingsStream != null) configBuilder.AddJsonStream(appSettingsStream);

        // Optionally use appSettings.Development.json to override values in
        // appSettings.json that shouldn't be committed to source control
        if (appSettingsDevStream != null) configBuilder.AddJsonStream(appSettingsDevStream);

        //if (configBuilder.Sources.Count > 0) builder.Configuration.AddConfiguration(configBuilder.Build());
        var config = configBuilder.Build();

        //var config = new ConfigurationBuilder()
        //            .AddJsonFile(new EmbeddedFileProvider(assembly), "appsettings.json", optional: true, false)
        //            //.AddJsonStream(stream)
        //            .Build();

        //      try
        //      {
        builder.Configuration.AddConfiguration(config);
        builder.Services.AddMauiBlazorWebView();
        //      }
        //      catch (Exception ex) 
        //{
        //	Debug.WriteLine(ex);
        //}
#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        var dbPath = FileAccessHelper.GetLocalFilePath(Constants.LocalDbFile);

        builder.Services.AddSingleton<IDataService>(
            s => ActivatorUtilities.CreateInstance<DataService>(s, dbPath));

        builder.Services.AddScoped<AuthenticationStateProvider, ExternalAuthenticationStateProvider>();
        builder.Services.AddScoped<IExternalAuthenticationStateProvider, ExternalAuthenticationStateProvider>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<IGraphService, GraphService>();
        builder.Services.AddSingleton<IBooksDataService, BooksDataService>();

        builder.Services.AddRazorClassLibraryServices(config);

        return builder.Build();
    }
}
