using BookShelves.Maui.Helpers;
using BookShelves.Maui.Services;
using BookShelves.Shared;
using BookShelves.Shared.DataInterfaces;
using CommunityToolkit.Maui;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace BookShelves.Maui;

public static class MauiProgramExtensions
{

	public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
	{
        //IConfiguration configuration;

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseMauiCommunityToolkit();

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

        builder.Services.AddTransient<IBooksDataService, BooksDataService>();

        builder.Services.AddScoped<AuthenticationStateProvider, ExternalAuthenticationStateProvider>();
        builder.Services.AddScoped<IExternalAuthenticationStateProvider, ExternalAuthenticationStateProvider>();
        builder.Services.AddScoped<IAuthService, Services.AuthService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<IAuthenticationService, AuthenticationService>();
        builder.Services.AddSingleton<IGraphService, GraphService>();

        //var provider = new ExternalAuthenticationStateProvider();
        //builder.Services.AddSingleton(s => provider);
        //builder.Services.AddSingleton<AuthenticationStateProvider>(s => provider);
        //DependencyService.RegisterSingleton<AuthenticationStateProvider>(provider);

        //builder.Services.AddSingleton<IExternalAuthenticationStateProvider>(provider);

        builder.Services.AddRazorClassLibraryServices(config);

        return builder;
	}
}
