// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BookShelves.Shared.DataInterfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Maui.ApplicationModel;

namespace BookShelves.Maui2.Services;

public partial class AuthenticationService : ObservableObject, IAuthenticationService, IAuthenticationProvider, IIdToken
{
    private readonly Lazy<Task<IPublicClientApplication>> _pca;
    private readonly ISettingsService _settingsService;
    private readonly IWindowService? _windowService;

    private string _userIdentifier = string.Empty;
    private ClaimsPrincipal _currentPrincipal;

    public GraphServiceClient GraphClient => new(this);
    public ClaimsPrincipal CurrentPrincipal => _currentPrincipal;

    private bool _isSignedIn = false;
    public bool IsSignedIn
    {
        get => _isSignedIn;
        private set
        {
            _isSignedIn = value;
            OnPropertyChanged();
        }
    }

    public AuthenticationService(ISettingsService settingsService, IWindowService? windowService)
    {
        _pca = new Lazy<Task<IPublicClientApplication>>(InitializeMsalWithCache);
        _settingsService = settingsService;
        _windowService = windowService;
        _currentPrincipal = new ClaimsPrincipal();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Attempts to get a IIdToken silently from the cache. If this fails, the user needs to sign in.
    /// </remarks>
    public async Task<bool> IsAuthenticatedAsync()
    {
        var account = await GetUserAccountAsync();
        var silentResult = await GetTokenSilentlyAsync(account);

        SetCurrentPrincipal(silentResult);

        IsSignedIn = silentResult is not null;
        return IsSignedIn;
    }

    public async Task<bool> SignInAsync()
    {
        var account = await GetUserAccountAsync();
        
        // First attempt to get a IIdToken silently
        var result = await GetTokenSilentlyAsync(account);
        
        // If silent attempt didn't work, try an
        // interactive sign in
        result ??= await GetTokenInteractivelyAsync(account);

        SetCurrentPrincipal(result);

        IsSignedIn = result is not null;
        return IsSignedIn;
    }

    private void SetCurrentPrincipal(AuthenticationResult? result)
    {
        var idToken = result?.IdToken;
        //var accessToken = result?.AccessToken;
        if (idToken != null)
        {
            var handler = new JwtSecurityTokenHandler();
            var idData = handler.ReadJwtToken(idToken);
            _currentPrincipal = new ClaimsPrincipal(new ClaimsIdentity(idData.Claims, "TEST", "name", "roles"));
        }
        else
        {
            _currentPrincipal = new ClaimsPrincipal();
        }
    }

    public async Task SignOutAsync()
    {
        var pca = await _pca.Value;

        // Get all accounts (there should only be one)
        // and remove them from the cache
        var accounts = await pca.GetAccountsAsync();
        foreach (var account in accounts)
        {
            await pca.RemoveAsync(account);
        }

        // Clear the user identifier
        _userIdentifier = string.Empty;
        _currentPrincipal = new ClaimsPrincipal();
        IsSignedIn = false;
    }

    /// <summary>
    /// Initializes a PublicClientApplication with a secure serialized cache.
    /// </summary>
    private async Task<IPublicClientApplication> InitializeMsalWithCache()
    {
        var brokerOptions = new BrokerOptions(BrokerOptions.OperatingSystems.Windows)
        {
            Title = "BookShelves"
        };

        // Initialize the PublicClientApplication
        var builder = PublicClientApplicationBuilder
            .Create(_settingsService.ClientId)
            .WithAuthority(_settingsService.AzureAdAuthority)
            .WithBroker(brokerOptions)
            .WithParentActivityOrWindow(_windowService?.GetMainWindowHandle())
            .WithRedirectUri($"msal{Constants.ApplicationId}://auth");

        builder = AddPlatformConfiguration(builder);

        var pca = builder.Build();

        await RegisterMsalCacheAsync(pca.UserTokenCache);

        return pca;
    }

    private static PublicClientApplicationBuilder AddPlatformConfiguration(PublicClientApplicationBuilder builder)
    {
        if (OperatingSystem.IsWindows())
        {
            //builder.WithWindowsEmbeddedBrowserSupport();
            //builder.WithDesktopFeatures();
            return builder;
        }
        else // handle mac, ios, android
        {
            // from example code: https://github.com/microsoftgraph/msgraph-sample-maui/blob/main/GraphMAUI/Platforms/Windows/AuthenticationService.cs
            // ios:  return builder.WithIosKeychainSecurityGroup("com.microsoft.adalcache");
            // android: return builder.WithParentActivityOrWindow(() => Platform.CurrentActivity);
            // windows: return builder;
            return builder;
        }
    }


    //        private partial Task RegisterMsalCacheAsync(ITokenCache tokenCache)

    private async Task RegisterMsalCacheAsync(ITokenCache tokenCache)
    {
        if (OperatingSystem.IsWindows())
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
        else // handle android, mac, ios
        {
            // from example code here: https://github.com/microsoftgraph/msgraph-sample-maui/blob/main/GraphMAUI/Platforms/iOS/AuthenticationService.cs
            //ios: return Task.CompletedTask;
            //android: return Task.CompletedTask;
            //windows: above code
            //Task.CompletedTask;
            return; // Task.CompletedTask;
        }
    }
    //{
    //    // Configure storage properties for cross-platform
    //    // See https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/wiki/Cross-platform-Token-Cache
    //    var storageProperties =
    //        new StorageCreationPropertiesBuilder(_settingsService.CacheFileName, _settingsService.CacheDirectory)
    //        .WithLinuxKeyring(
    //            _settingsService.LinuxKeyRingSchema,
    //            _settingsService.LinuxKeyRingCollection,
    //            _settingsService.LinuxKeyRingLabel,
    //            _settingsService.LinuxKeyRingAttr1,
    //            _settingsService.LinuxKeyRingAttr2)
    //        .WithMacKeyChain(
    //            _settingsService.KeyChainServiceName,
    //            _settingsService.KeyChainAccountName)
    //        .Build();

    //    // Create a cache helper
    //    var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);

    //    // Connect the PublicClientApplication's cache with the cacheHelper.
    //    // This will cause the cache to persist into secure storage on the device.
    //    cacheHelper.RegisterCache(tokenCache);
    //}

    /// <summary>
    /// Get the user account from the MSAL cache.
    /// </summary>
    private async Task<IAccount?> GetUserAccountAsync()
    {
        try
        {
            var pca = await _pca.Value;

            if (string.IsNullOrEmpty(_userIdentifier))
            {
                // If no saved user ID, then get the first account.
                // There should only be one account in the cache anyway.
                var accounts = await pca.GetAccountsAsync();
                var account = accounts.FirstOrDefault();

                account ??= PublicClientApplication.OperatingSystemAccount;

                // Save the user ID so this is easier next time
                _userIdentifier = account?.HomeAccountId.Identifier ?? string.Empty;
                return account;
            }

            // If there's a saved user ID use it to get the account
            return await pca.GetAccountAsync(_userIdentifier);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempt to acquire a IIdToken silently (no prompts).
    /// </summary>
    private async Task<AuthenticationResult?> GetTokenSilentlyAsync(IAccount? userAccount)
    {
        try
        {
            var pca = await _pca.Value;

            if (userAccount == null) return null;

            return await pca.AcquireTokenSilent(_settingsService.GraphScopes, userAccount)
                .ExecuteAsync();
        }
        catch (MsalUiRequiredException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to get a IIdToken interactively using the device's browser.
    /// </summary>
    private async Task<AuthenticationResult> GetTokenInteractivelyAsync(IAccount? userAccount)
    {
        var pca = await _pca.Value;
        //var window = _windowService.GetMainWindowHandle();

        var result = await pca.AcquireTokenInteractive(_settingsService.GraphScopes)
            //.WithAccount(userAccount)
            //.WithLoginHint("derek_m@outlook.com")
            .WithPrompt(Prompt.NoPrompt)
            .WithParentActivityOrWindow(_windowService?.GetMainWindowHandle())
            //.WithUseEmbeddedWebView(true)
            .ExecuteAsync();

        // Store the user ID to make account retrieval easier
        _userIdentifier = result.Account.HomeAccountId.Identifier;
        return result;
    }

    public async Task AuthenticateRequestAsync(
        RequestInformation request,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        if (request.URI.Host == "graph.microsoft.com")
        {
            var account = await GetUserAccountAsync();

            // First try to get the IIdToken silently
            var result = await GetTokenSilentlyAsync(account);
            
            // If silent acquisition fails, try interactive
            result ??= await GetTokenInteractivelyAsync(account);

            request.Headers.Add("Authorization", $"Bearer {result.AccessToken}");
        }
    }
}