using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;

namespace BookShelves.Maui.Services;

public partial class AuthenticationService
{
    private partial PublicClientApplicationBuilder AddPlatformConfiguration(PublicClientApplicationBuilder builder)
    {
        return builder.WithParentActivityOrWindow(_windowService?.GetMainWindowHandle());
    }

    private partial AcquireTokenInteractiveParameterBuilder AddAquireTokenPlatformConfiguration(AcquireTokenInteractiveParameterBuilder builder)
    {
        builder.WithUseEmbeddedWebView(true);
        return builder;
    }

    private partial Task RegisterMsalCacheAsync(ITokenCache tokenCache)
    {
        return Task.CompletedTask;
    }
}