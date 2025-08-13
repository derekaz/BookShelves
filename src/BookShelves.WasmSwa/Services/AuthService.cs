using BookShelves.Shared.ServiceInterfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BookShelves.WasmSwa.Services;

public class AuthService(IServiceProvider serviceProvider) : IAuthService
{
    private IServiceProvider _serviceProvider = serviceProvider;

    public Task InitializeAsync()
    {
        //throw new NotImplementedException();
        return Task.CompletedTask;
    }

    public async Task LoginAsync()
    {
        await Task.Run(BeginLogin);
    }

    public async Task LogoutAsync()
    {
        await Task.Run(BeginLogout);
    }

    private void BeginLogin()
    {
        var navigationManager = _serviceProvider.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo("authentication/login");
    }

    private void BeginLogout()
    {
        var navigationManager = _serviceProvider.GetRequiredService<NavigationManager>();
        navigationManager.NavigateToLogout("authentication/logout", "/");
    }
}
