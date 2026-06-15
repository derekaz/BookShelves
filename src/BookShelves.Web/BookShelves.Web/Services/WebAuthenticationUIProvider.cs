using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Web.Services;

public class WebAuthenticationUIProvider : IAuthenticationUIProvider
{
    public string GetLoginUrl() => "MicrosoftIdentity/Account/SignIn";

    public string GetLogoutUrl() => "MicrosoftIdentity/Account/SignOut";

    public bool RequiresNavigation => true;

    public string PlatformName => "Web";
}
