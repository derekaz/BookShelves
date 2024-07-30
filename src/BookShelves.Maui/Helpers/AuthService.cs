//using Microsoft.Identity.Client;

//namespace BookShelves.Maui.Helpers
//{
//    internal class AuthService
//    {
//        private readonly IPublicClientApplication authenticationClient;
//        private string tenantId = "d23695ff-4843-44ec-8c6f-af2de0c2ccc8";
//        public AuthService()
//        {
//            try
//            {
//                authenticationClient = PublicClientApplicationBuilder
//                    .Create(Constants.ApplicationId)
//                    .WithAuthority(AzureCloudInstance.AzurePublic, tenantId)
//                    //.WithAuthority(AzureCloudInstance.AzurePublic, "common")
//                    //.WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdAndPersonalMicrosoftAccount)
//                    //.WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
//                    //.WithRedirectUri($"msal{Constants.ApplicationId}://auth")
//                    .WithRedirectUri("http://localhost")
//                    .Build();
//            }
//            catch (Exception e) 
//            { 
//            }
//        }

//        public async Task<AuthenticationResult> LoginAsync(CancellationToken cancellationToken)
//        {
//            AuthenticationResult result;
//            IEnumerable<IAccount> accounts = await authenticationClient.GetAccountsAsync().ConfigureAwait(false);

//            try
//            {
//                IAccount firstAccount = accounts.FirstOrDefault();
//                result = await authenticationClient
//                    .AcquireTokenSilent(Constants.Scopes, firstAccount)
//                    .ExecuteAsync(cancellationToken)
//                    .ConfigureAwait(false);
//            } catch (MsalUiRequiredException)
//            {
//                try
//                {
//                    var builder = authenticationClient
//                        .AcquireTokenInteractive(Constants.Scopes)
//                        .WithTenantId(tenantId)
//                        .WithUseEmbeddedWebView(true);
//                        //.WithParentActivityOrWindow(App.ParentWindow);
//                    //.WithParentActivityOrWindow(App.ParentWindow);

//                    if (Device.RuntimePlatform != "UWP")
//                    {
//                        // on Android and iOS, prefer to use the system browser, which does not exist on UWP
//                        SystemWebViewOptions systemWebViewOptions = new SystemWebViewOptions()
//                        {
//                            iOSHidePrivacyPrompt = true,
//                        };

//                        builder.WithSystemWebViewOptions(systemWebViewOptions);
//                        builder.WithUseEmbeddedWebView(false);
//                    }

//                    result = await builder.ExecuteAsync().ConfigureAwait(false);
//                    return result;
//                }
//                catch (Exception ex2)
//                {
//                    //Device.BeginInvokeOnMainThread(async () =>
//                    //{
//                    //    await DisplayAlert("Acquire token interactive failed. See exception message for details: ", ex2.Message, "Dismiss");
//                    //});
//                }

//            }

//            //result = await authenticationClient
//            //        .AcquireTokenInteractive(Constants.Scopes)
//            //        .WithPrompt(Prompt.ForceLogin)
//            //        .ExecuteAsync(cancellationToken);
//            //    return result;
//            //}
//            //catch (MsalClientException ex)
//            //{
//            //    return null;
//            //}
//            return null;
//        }

//        internal async Task LogoutAsync()
//        {
//            IEnumerable<IAccount> accounts = await authenticationClient.GetAccountsAsync().ConfigureAwait(false);
//            var existingUser = accounts.SingleOrDefault();
//            await this.LogoutAsync(existingUser).ConfigureAwait(false);
//        }

//        internal async Task LogoutAsync(IAccount user)
//        {
//            await authenticationClient.RemoveAsync(user).ConfigureAwait(false);
//        }
//    }
//}
