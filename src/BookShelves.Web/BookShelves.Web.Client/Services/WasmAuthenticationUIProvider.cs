using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Web.Client.Services;

public class WasmAuthenticationUIProvider : IAuthenticationUIProvider
{
    public string GetLoginUrl() => "authentication/login";

    public string GetLogoutUrl() => "authentication/logout";

    public bool RequiresNavigation => true;

    public string PlatformName => "Web";
}
