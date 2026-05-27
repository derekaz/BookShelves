namespace BookShelves.Shared.Services.ServiceInterfaces;

public interface IAuthService
{
    Task InitializeAsync();

    Task LoginAsync();

    Task LogoutAsync();
}
