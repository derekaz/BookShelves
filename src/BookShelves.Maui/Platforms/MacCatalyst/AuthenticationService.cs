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

    private partial AcquireTokenInteractiveParameterBuilder AddAquireTokenPlatformConfiguration(AcquireTokenInteractiveParameterBuilder builder)
    {
        builder.WithUseEmbeddedWebView(false);
        return builder;
    }

    private partial Task RegisterMsalCacheAsync(ITokenCache tokenCache)
    {
        return Task.CompletedTask;
    }
}