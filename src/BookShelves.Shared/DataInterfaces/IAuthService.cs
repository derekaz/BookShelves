namespace BookShelves.Shared.DataInterfaces;

public interface IAuthService
{
    Task InitializeAsync();

    Task LoginAsync();

    Task LogoutAsync();
}
