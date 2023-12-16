using Microsoft.Extensions.Logging;
using BookShelves.Data;
using BookShelves.Helpers;
using Blazored.Modal;

namespace BookShelves;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        var dbPath = FileAccessHelper.GetLocalFilePath(Constants.LocalDbFile);

        var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<WeatherForecastService>();

        builder.Services.AddSingleton<IDataService>(
            s => ActivatorUtilities.CreateInstance<DataService>(s, dbPath));

        builder.Services.AddScoped<IBooksDataService, BooksDataService>();
        //builder.Services.AddTransient<BookDataService>();

        builder.Services.AddBlazoredModal();

        return builder.Build();
	}
}
