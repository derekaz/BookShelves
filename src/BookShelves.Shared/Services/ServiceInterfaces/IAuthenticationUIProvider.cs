namespace BookShelves.Shared.Services.ServiceInterfaces;

public interface IAuthenticationUIProvider
{
    string GetLoginUrl();

    string GetLogoutUrl();

    bool RequiresNavigation { get; }

    string PlatformName { get; }
}
