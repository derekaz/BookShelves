using Blazored.Modal;
using BookShelves.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using System.Diagnostics;
using System.Reflection;

namespace BookShelves.Maui;

public static class MauiProgramExtensions
{

	public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
	{
		IConfiguration configuration;

        builder
            .UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        var a = Assembly.GetExecutingAssembly();
        //using var stream = a.GetManifestResourceStream("MauiApp27.appsettings.json");

        var config = new ConfigurationBuilder()
                    .AddJsonFile(new EmbeddedFileProvider(a), "appsettings.json", optional: true, false)
                    //.AddJsonStream(stream)
                    .Build();

        try
        {
            builder.Configuration.AddConfiguration(config);
            builder.Services.AddMauiBlazorWebView();
		}
		catch (Exception ex) 
		{
			Debug.WriteLine(ex);
		}
#if DEBUG
		builder.Logging.AddDebug();
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        var dbPath = FileAccessHelper.GetLocalFilePath(Constants.LocalDbFile);

        builder.Services.AddSingleton<IDataService>(
            s => ActivatorUtilities.CreateInstance<DataService>(s, dbPath));

        builder.Services.AddScoped<IBooksDataService, BooksDataService>();

		builder.Services.AddRazorClassLibraryServices(config);

        return builder;
	}
}
