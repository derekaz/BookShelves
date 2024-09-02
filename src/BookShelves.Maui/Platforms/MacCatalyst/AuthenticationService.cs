using CoreFoundation;
using Microsoft.Identity.Client;

namespace BookShelves.Maui.Services;

public partial class AuthenticationService
{
    private partial PublicClientApplicationBuilder AddPlatformConfiguration(PublicClientApplicationBuilder builder)
    {
        builder.WithRedirectUri("http://localhost");
        //builder.WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient");
        //builder.WithDefaultRedirectUri();
        builder.WithIosKeychainSecurityGroup("com.microsoft.adalcache");
        return builder;
    }

    private async Task TryThisAsync(Uri uri)
    {
        if (uri == null)
        {
            throw new ArgumentNullException(nameof(uri));
        }
        string url = uri.AbsoluteUri;
        Console.WriteLine("AuthenticationService:TryThisAsync (Mac) - Before Url Open (url={0})", url);
        DispatchQueue.MainQueue.DispatchAsync(async () => 
        { 
            await Browser.Default.OpenAsync(url, BrowserLaunchMode.External); 
        });
        
        //await Browser.Default.OpenAsync(url);
        Console.WriteLine("AuthenticationService:TryThisAsync (Mac) - After Url Open");
        //Process.Start(new ProcessStartInfo("cmd", $"/c start microsoft-edge:{url}") { CreateNoWindow = true });
        //await Task.FromResult(0).ConfigureAwait(false);
    }

    private partial AcquireTokenInteractiveParameterBuilder AddAquireTokenPlatformConfiguration(AcquireTokenInteractiveParameterBuilder builder)
    {
        var options = new SystemWebViewOptions()
        {
            OpenBrowserAsync = TryThisAsync
        };
        builder.WithSystemWebViewOptions(options);
        //builder.WithUseEmbeddedWebView(false);
        return builder;
    }

    private partial Task RegisterMsalCacheAsync(ITokenCache tokenCache)
    {
        if (_dataProtector == null) throw new NullReferenceException(nameof(_dataProtector));
        MacTokenCacheHelper.EnableSerialization(tokenCache, _dataProtector);

        Console.WriteLine("AuthenticationService:InitializeMsalWithCache (Mac)-RegisterCache complete");
        return Task.CompletedTask;
    }
}