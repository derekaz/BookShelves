using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Identity.Client;

namespace BookShelves.Maui.Services;

public partial class AuthenticationService
{
    private partial PublicClientApplicationBuilder AddPlatformConfiguration(PublicClientApplicationBuilder builder)
    {
        //builder.WithWindowsEmbeddedBrowserSupport();
        //builder.WithDesktopFeatures();
        return builder.WithParentActivityOrWindow(_windowService?.GetMainWindowHandle());
    }


    private partial async Task RegisterMsalCacheAsync(ITokenCache tokenCache)
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

        // Create a cache helper
        var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);

        // Connect the PublicClientApplication's cache with the cacheHelper.
        // This will cause the cache to persist into secure storage on the device.
        cacheHelper.RegisterCache(tokenCache);
    }
}
