using BookShelves.Shared.DataInterfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace BookShelves.Maui.Services;

internal class AuthService(IServiceProvider serviceProvider) : IAuthService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task InitializeAsync()
    {
        var authenticationStateProvider = _serviceProvider.GetRequiredService<AuthenticationStateProvider>();
        await ((IExternalAuthenticationStateProvider)authenticationStateProvider).InitializeAsync();
    }

    public async Task LoginAsync()
    {
        var authenticationStateProvider = _serviceProvider.GetRequiredService<AuthenticationStateProvider>();
        await ((IExternalAuthenticationStateProvider)authenticationStateProvider).LogInAsync();
    }

    public async Task LogoutAsync()
    {
        var authenticationStateProvider = _serviceProvider.GetRequiredService<AuthenticationStateProvider>();
        await ((IExternalAuthenticationStateProvider)authenticationStateProvider).LogoutAsync();
    }
}
