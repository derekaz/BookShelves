using BookShelves.Shared.DataInterfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace BookShelves.Maui.Services;

internal class AuthService : IAuthService
{
    private IServiceProvider _serviceProvider;
    public AuthService(IServiceProvider serviceProvider) 
    { 
        _serviceProvider = serviceProvider;
    }

    public async Task InitializeAsync()
    {
        var authenticationStateProvider = _serviceProvider.GetRequiredService<AuthenticationStateProvider>();
        await ((IExternalAuthenticationStateProvider)authenticationStateProvider).InitializeAsync();
    }

    public async Task LoginAsync()
    {
        //var test = _serviceProvider.GetRequiredService<IAuthenticationService>();
        //await test.SignInAsync();
        var authenticationStateProvider = _serviceProvider.GetRequiredService<AuthenticationStateProvider>();
        await ((IExternalAuthenticationStateProvider)authenticationStateProvider).LogInAsync();
    }

    public async Task LogoutAsync()
    {
        //var test = _serviceProvider.GetRequiredService<IAuthenticationService>();
        //await test.SignOutAsync();

        var authenticationStateProvider = _serviceProvider.GetRequiredService<AuthenticationStateProvider>();
        await ((IExternalAuthenticationStateProvider)authenticationStateProvider).LogoutAsync();
    }
}
