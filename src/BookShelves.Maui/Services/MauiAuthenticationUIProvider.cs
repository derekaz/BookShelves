using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Maui.Services;

internal class MauiAuthenticationUIProvider : IAuthenticationUIProvider
{
    public string GetLoginUrl() => string.Empty;

    public string GetLogoutUrl() => string.Empty;

    public bool RequiresNavigation => false;

    public string PlatformName => "MAUI";
}
