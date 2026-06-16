using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Web.Services;

public class WebAuthenticationUIProvider : IAuthenticationUIProvider
{
    AuthenticationUIActionType IAuthenticationUIProvider.LoginActionType => AuthenticationUIActionType.Link;

    public string GetLoginUrl() => "MicrosoftIdentity/Account/SignIn"; // "authentication/login";

    AuthenticationUIActionType IAuthenticationUIProvider.LogoutActionType => AuthenticationUIActionType.Link; //.Form;

    public string GetLogoutUrl() => "MicrosoftIdentity/Account/SignOut"; //"authentication/logout";

    public bool RequiresNavigation => true;

    public string PlatformName => "Web";
}
