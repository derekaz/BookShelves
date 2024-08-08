using Microsoft.Identity.Client;

namespace BookShelves.Maui.Services;

public partial class AuthenticationService
{
    private partial PublicClientApplicationBuilder AddPlatformConfiguration(PublicClientApplicationBuilder builder)
    {
        builder.WithRedirectUri("msauth.com.azmoore.bookshelves.maui://auth");
        return builder.WithIosKeychainSecurityGroup("com.microsoft.adalcache");
    }

    private partial Task RegisterMsalCacheAsync(ITokenCache tokenCache)
    {
        return Task.CompletedTask;
    }
}