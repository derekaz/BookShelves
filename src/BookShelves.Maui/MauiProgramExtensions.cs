using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BookShelves;

public static class MauiProgramExtensions
{
	public static MauiAppBuilder UseSharedMauiApp(this MauiAppBuilder builder)
	{
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		try
		{
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

		return builder;
	}
}
