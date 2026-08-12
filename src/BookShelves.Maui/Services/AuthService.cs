using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Maui.Services;

internal class AuthService(IExternalAuthenticationStateProvider authenticationStateProvider) : IAuthService
{
    private readonly IExternalAuthenticationStateProvider _authenticationStateProvider = authenticationStateProvider;

    public async Task InitializeAsync()
    {
        await _authenticationStateProvider.InitializeAsync();
    }

    public async Task LoginAsync()
    {
        await _authenticationStateProvider.LogInAsync();
    }

    public async Task LogoutAsync()
    {
        await _authenticationStateProvider.LogoutAsync();
    }
}
