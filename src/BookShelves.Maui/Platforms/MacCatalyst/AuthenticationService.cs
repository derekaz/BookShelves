using CoreFoundation;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace BookShelves.Maui.Services;

public partial class AuthenticationService
{
    private partial PublicClientApplicationBuilder AddPlatformConfiguration(PublicClientApplicationBuilder builder)
    {
        builder.WithRedirectUri("http://localhost");
        builder.WithIosKeychainSecurityGroup("com.microsoft.adalcache");
        return builder;
    }

    private async Task TryThisAsync(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);

        await Task.Delay(10);

        string url = uri.AbsoluteUri;
        _logger.LogInformation("AuthenticationService:TryThisAsync (Mac) - Before Url Open (url={url})", url);
        DispatchQueue.MainQueue.DispatchAsync(async () =>
        { 
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.External); 
        });

        _logger.LogInformation("AuthenticationService:TryThisAsync (Mac) - After Url Open");
    }

    private partial AcquireTokenInteractiveParameterBuilder AddAcquireTokenPlatformConfiguration(AcquireTokenInteractiveParameterBuilder builder)
    {
        var options = new SystemWebViewOptions()
        {
            OpenBrowserAsync = TryThisAsync
        };
        builder.WithSystemWebViewOptions(options);
        return builder;
    }

    private partial Task RegisterMsalCacheAsync(ITokenCache tokenCache)
    {
        MacTokenCacheHelper.EnableSerialization(tokenCache);

        _logger.LogInformation("AuthenticationService:InitializeMsalWithCache (Mac)-RegisterCache complete");
        return Task.CompletedTask;
    }
}