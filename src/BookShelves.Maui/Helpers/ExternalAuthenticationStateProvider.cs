using BookShelves.Shared.ServiceInterfaces;
using CommunityToolkit.Maui.Alerts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace BookShelves.Maui.Helpers;

public class ExternalAuthenticationStateProvider : AuthenticationStateProvider, IExternalAuthenticationStateProvider
{
    private ClaimsPrincipal currentUser = new ClaimsPrincipal(new ClaimsIdentity());
    private IAuthenticationService _authenticationService;

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return await Task.FromResult(new AuthenticationState(currentUser));
    }

    public ExternalAuthenticationStateProvider(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public async Task InitializeAsync()
    {
        var loginTask = await _authenticationService.IsAuthenticatedAsync();
        if (loginTask)
        {
            currentUser = _authenticationService.CurrentPrincipal;
            var state = Task.FromResult(new AuthenticationState(currentUser));
            NotifyAuthenticationStateChanged(state);
            await state;
        }
        return;
    }

    public Task LogInAsync()
    {
        var loginTask = LogInAsyncCore();
        NotifyAuthenticationStateChanged(loginTask);

        return loginTask;

        async Task<AuthenticationState> LogInAsyncCore()
        {
            var user = await LoginWithExternalProviderAsync();
            currentUser = user;
            return new AuthenticationState(currentUser);
        }
    }

    private async Task<ClaimsPrincipal> LoginWithExternalProviderAsync()
    {
        try
        {
            if (_authenticationService == null) { throw new InvalidOperationException("AuthenticationService not defined"); }

            var result = await _authenticationService.SignInAsync();
            if (result) return _authenticationService.CurrentPrincipal;
        }
        catch (MsalClientException ex)
        {
            //await Toast.Make(ex.Message).Show();
        }
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
        return authenticatedUser;
    }

    public async Task LogoutAsync()
    {
        await _authenticationService.SignOutAsync();
        currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(currentUser)));
    }
}
