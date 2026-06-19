namespace BookShelves.Shared.Services.ServiceInterfaces;

public interface IAuthenticationUIProvider
{
    AuthenticationUIActionType LoginActionType { get; }

    string GetLoginUrl();

    AuthenticationUIActionType LogoutActionType { get; }

    string GetLogoutUrl();

    bool RequiresNavigation { get; }

    string PlatformName { get; }
}

public enum AuthenticationUIActionType
{
    Code,
    Link,
    Form,
    AuthenticationServiceShim
}
