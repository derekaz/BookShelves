namespace BookShelves.Shared.DataInterfaces;

public interface IAuthService
{
    Task LoginAsync();

    Task LogoutAsync();
}
