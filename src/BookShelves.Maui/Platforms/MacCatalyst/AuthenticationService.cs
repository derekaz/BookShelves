using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

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
        Console.WriteLine("AuthenticationService:TryThisAsync (Mac) - Before Url Open");
        url = url.Replace("&", "^&");
        await Browser.OpenAsync(url);
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
        //builder.With
        builder.WithSystemWebViewOptions(options);
        //builder.WithUseEmbeddedWebView(false);
        return builder;
    }

    private partial Task RegisterMsalCacheAsync(ITokenCache tokenCache)
    {
        MacTokenCacheHelper.EnableSerialization(tokenCache);

        // Configure storage properties for cross-platform
        // See https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/wiki/Cross-platform-Token-Cache
        //var storageProperties =
        //    new StorageCreationPropertiesBuilder(_settingsService.CacheFileName, _settingsService.CacheDirectory)
        //    .WithLinuxKeyring(
        //        _settingsService.LinuxKeyRingSchema,
        //        _settingsService.LinuxKeyRingCollection,
        //        _settingsService.LinuxKeyRingLabel,
        //        _settingsService.LinuxKeyRingAttr1,
        //        _settingsService.LinuxKeyRingAttr2)
        //    .WithMacKeyChain(
        //        _settingsService.KeyChainServiceName,
        //        _settingsService.KeyChainAccountName)
        //    .Build();

        //// Create a cache helper
        //var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
        //Console.WriteLine("AuthenticationService:InitializeMsalWithCache (Mac)-MsalCacheHelper CreateAsync complete");

        //// Connect the PublicClientApplication's cache with the cacheHelper.
        //// This will cause the cache to persist into secure storage on the device.
        //cacheHelper.RegisterCache(tokenCache);
        Console.WriteLine("AuthenticationService:InitializeMsalWithCache (Mac)-RegisterCache complete");
        return Task.CompletedTask;
    }
}