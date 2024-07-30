//using BookShelves.Maui.Services;
//using Microsoft.Graph;
//using Microsoft.Identity.Client;
//using Microsoft.Kiota.Abstractions.Authentication;
//using Microsoft.Kiota.Abstractions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BookShelves.Maui.Services
//{
//    internal class AuthenticationService
//    {
//    }
//}


// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using BookShelves.Shared.DataInterfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static SQLite.SQLite3;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BookShelves.Maui.Services
{
    public partial class AuthenticationService : ObservableObject, IAuthenticationService, IAuthenticationProvider, IIdToken
    {
        private Lazy<Task<IPublicClientApplication>> _pca;
        private string _userIdentifier = string.Empty;
        private ISettingsService _settingsService;
        private ClaimsPrincipal _currentPrincipal;

        public GraphServiceClient GraphClient => new GraphServiceClient(this);
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

        public AuthenticationService(ISettingsService settingsService)
        {
            _pca = new Lazy<Task<IPublicClientApplication>>(InitializeMsalWithCache);
            _settingsService = settingsService;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Attempts to get a IIdToken silently from the cache. If this fails, the user needs to sign in.
        /// </remarks>
        public async Task<bool> IsAuthenticatedAsync()
        {
            var silentResult = await GetTokenSilentlyAsync();

            SetCurrentPrincipal(silentResult);

            IsSignedIn = silentResult is not null;
            return IsSignedIn;
        }

        public async Task<bool> SignInAsync()
        {
            // First attempt to get a IIdToken silently
            var result = await GetTokenSilentlyAsync();
            if (result == null)
            {
                // If silent attempt didn't work, try an
                // interactive sign in
                result = await GetTokenInteractivelyAsync();
            }

            SetCurrentPrincipal(result);

            IsSignedIn = result is not null;
            return IsSignedIn;
        }

        private void SetCurrentPrincipal(AuthenticationResult? result)
        {
            var idToken = result?.IdToken;
            var accessToken = result?.AccessToken;
            if (idToken != null)
            {
                var handler = new JwtSecurityTokenHandler();
                var idData = handler.ReadJwtToken(idToken);
                //var accessdata = handler.ReadJwtToken(accessToken);
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
            // Initialize the PublicClientApplication
            var builder = PublicClientApplicationBuilder
                .Create(_settingsService.ClientId)
                .WithAuthority(_settingsService.AzureAdAuthority)
                .WithRedirectUri(_settingsService.RedirectUri);

            builder = AddPlatformConfiguration(builder);

            var pca = builder.Build();

            await RegisterMsalCacheAsync(pca.UserTokenCache);

            return pca;
        }

        private PublicClientApplicationBuilder AddPlatformConfiguration(PublicClientApplicationBuilder builder)
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
        private async Task<IAccount> GetUserAccountAsync()
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
        private async Task<AuthenticationResult> GetTokenSilentlyAsync()
        {
            try
            {
                var pca = await _pca.Value;

                var account = await GetUserAccountAsync();
                if (account == null) return null;

                return await pca.AcquireTokenSilent(_settingsService.GraphScopes, account)
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
        private async Task<AuthenticationResult> GetTokenInteractivelyAsync()
        {
            var pca = await _pca.Value;

            var result = await pca.AcquireTokenInteractive(_settingsService.GraphScopes)
                //.WithUseEmbeddedWebView(true)
                .ExecuteAsync();

            // Store the user ID to make account retrieval easier
            _userIdentifier = result.Account.HomeAccountId.Identifier;
            return result;
        }

        public async Task AuthenticateRequestAsync(
            RequestInformation request,
            Dictionary<string, object> additionalAuthenticationContext = null,
            CancellationToken cancellationToken = default)
        {
            if (request.URI.Host == "graph.microsoft.com")
            {
                // First try to get the IIdToken silently
                var result = await GetTokenSilentlyAsync();
                if (result == null)
                {
                    // If silent acquisition fails, try interactive
                    result = await GetTokenInteractivelyAsync();
                }

                request.Headers.Add("Authorization", $"Bearer {result.AccessToken}");
            }
        }
    }
}