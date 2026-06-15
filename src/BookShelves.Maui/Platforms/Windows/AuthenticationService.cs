using Microsoft.Identity.Client;



//using Microsoft.Identity.Client.Broker;
using Microsoft.Identity.Client.Extensions.Msal;

namespace BookShelves.Maui.Services;


public partial class AuthenticationService
{
    private partial PublicClientApplicationBuilder AddPlatformConfiguration(PublicClientApplicationBuilder builder)
    {
        //var brokerOptions = new BrokerOptions(BrokerOptions.OperatingSystems.Windows)
        //{
        //    Title = "BookShelves"
        //};

        //builder.WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows));

        // // builder.WithBroker(new BrokerOptions(BrokerOptions.OperatingSystems.Windows) { Title = "BookShelves" });
        //builder.WithWinUI3EmbeddedBrowserSupport();
        //builder.WithWindowsDesktopFeatures(brokerOptions);
        // builder.WithWindowsEmbeddedBrowserSupport();

        // //builder.WithWindowsDesktopFeatures(new BrokerOptions(BrokerOptions.OperatingSystems.Windows));
        //builder.WithDefaultRedirectUri();
        //builder.WithParentActivityOrWindow(_windowService?.GetMainWindowHandle());
        //builder.WithBroker(brokerOptions);
        //builder.WithBroker(false);

        return builder;
    }


    private partial AcquireTokenInteractiveParameterBuilder AddAquireTokenPlatformConfiguration(AcquireTokenInteractiveParameterBuilder builder)
    {
        builder.WithUseEmbeddedWebView(true);
        return builder;
    }

    private async partial Task RegisterMsalCacheAsync(ITokenCache tokenCache)
    {
        // Configure storage properties for cross-platform
        // See https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/wiki/Cross-platform-Token-Cache
        var storageProperties =
            new StorageCreationPropertiesBuilder(_settingsService.CacheFileName, _settingsService.CacheDirectory)
            .WithLinuxKeyring(
                _settingsService.LinuxKeyRingSchema,
                _settingsService.LinuxKeyRingCollection,
                _settingsService.LinuxKeyRingLabel,
                _settingsService.LinuxKeyRingAttr1,
                _settingsService.LinuxKeyRingAttr2)
            .WithMacKeyChain(
                _settingsService.KeyChainServiceName,
                _settingsService.KeyChainAccountName)
            .Build();

        // CreateBook a cache helper
        var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);

        // Connect the PublicClientApplication's cache with the cacheHelper.
        // This will cause the cache to persist into secure storage on the device.
        cacheHelper.RegisterCache(tokenCache);
    }
}
