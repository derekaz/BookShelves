using Microsoft.Identity.Client;

namespace BookShelves.Maui.Services;

public partial class AuthenticationService
{
    private partial PublicClientApplicationBuilder AddPlatformConfiguration(PublicClientApplicationBuilder builder)
    {
        return builder.WithParentActivityOrWindow(_windowService?.GetMainWindowHandle());
    }

    private partial Task RegisterMsalCacheAsync(ITokenCache tokenCache)
    {
        return Task.CompletedTask;
    }
}