using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Web.Client.Services;

public class WasmAuthenticationUIProvider : IAuthenticationUIProvider
{
    AuthenticationUIActionType IAuthenticationUIProvider.LoginActionType => AuthenticationUIActionType.Link;

    public string GetLoginUrl() => "authentication/login";

    AuthenticationUIActionType IAuthenticationUIProvider.LogoutActionType => AuthenticationUIActionType.Form;

    public string GetLogoutUrl() => "authentication/logout";

    public bool RequiresNavigation => true;

    public string PlatformName => "Web";
}
