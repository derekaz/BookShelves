using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.WasmSwa.Services;

internal class WasmAuthenticationUIProvider : IAuthenticationUIProvider
{
    AuthenticationUIActionType IAuthenticationUIProvider.LoginActionType => AuthenticationUIActionType.Link;

    public string GetLoginUrl() => "authentication/login";

    AuthenticationUIActionType IAuthenticationUIProvider.LogoutActionType => AuthenticationUIActionType.AuthenticationServiceShim;

    public string GetLogoutUrl() => "authentication/logout";

    public bool RequiresNavigation => true;

    public string PlatformName => "WebAssembly-SWA";
}
