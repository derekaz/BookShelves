using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Maui.Services.Maui;

internal class AuthenticationUIProviderService : IAuthenticationUIProvider
{
    AuthenticationUIActionType IAuthenticationUIProvider.LoginActionType => AuthenticationUIActionType.Code;

    public string GetLoginUrl() => string.Empty;

    AuthenticationUIActionType IAuthenticationUIProvider.LogoutActionType => AuthenticationUIActionType.Code;

    public string GetLogoutUrl() => string.Empty;

    public bool RequiresNavigation => false;

    public string PlatformName => "MAUI";
}
