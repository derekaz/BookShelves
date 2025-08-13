namespace BookShelves.Shared.ServiceInterfaces;

public interface IAuthService
{
    Task InitializeAsync();

    Task LoginAsync();

    Task LogoutAsync();
}
