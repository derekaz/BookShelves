using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Web.Client.Services;

internal class ClientAuthenticationUIProvider : IAuthenticationUIProvider
{
    AuthenticationUIActionType IAuthenticationUIProvider.LoginActionType => AuthenticationUIActionType.Link;

    public string GetLoginUrl() => "MicrosoftIdentity/Account/SignIn";

    AuthenticationUIActionType IAuthenticationUIProvider.LogoutActionType => AuthenticationUIActionType.Link;

    public string GetLogoutUrl() => "MicrosoftIdentity/Account/SignOut";

    public bool RequiresNavigation => true;

    public string PlatformName => "Web";
}
