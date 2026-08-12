using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace BookShelves.Maui.Services;

public class ExternalAuthenticationStateProvider(
    IAuthenticationService authenticationService,
    ILogger<ExternalAuthenticationStateProvider> logger) : AuthenticationStateProvider, IExternalAuthenticationStateProvider
{
    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly ILogger<ExternalAuthenticationStateProvider> _logger = logger;
    private bool _hasCheckedAuthenticationState;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_hasCheckedAuthenticationState)
        {
            await RefreshCurrentUserAsync(notify: false);
        }

        return new AuthenticationState(_currentUser);
    }

    public async Task<string?> GetAccessTokenAsync(string[] scopes)
    {
        try
        {
            return await _authenticationService.GetAccessTokenAsync(scopes);
        }
        catch (MsalUiRequiredException)
        {
            // This exception means the refresh token has expired or been revoked.
            // You must force the user to re-authenticate interactively.
            return null;
        }
    }

    public async Task InitializeAsync()
    {
        await RefreshCurrentUserAsync(notify: true);
    }

    public Task LogInAsync()
    {
        var state = LogInAsyncCore();
        NotifyAuthenticationStateChanged(state);

        return state;

        async Task<AuthenticationState> LogInAsyncCore()
        {
            try
            {
                var user = await LoginWithExternalProviderAsync();
                _currentUser = user;
                _hasCheckedAuthenticationState = true;
                return new AuthenticationState(user);
            }
            catch (Exception ex)
            {
                _logger.LogError("ExternalAuthenticationStateProvider: LogInAsyncCore - Exception: {0}", ex);
                //await Toast.Make(ex.Message).Show();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
    }

    private async Task RefreshCurrentUserAsync(bool notify)
    {
        var isAuthenticated = await _authenticationService.IsAuthenticatedAsync();
        _currentUser = isAuthenticated
            ? _authenticationService.CurrentPrincipal
            : new ClaimsPrincipal(new ClaimsIdentity());
        _hasCheckedAuthenticationState = true;

        if (notify)
        {
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
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
            _logger.LogError("ExternalAuthenticationStateProvider: LogingWithExternalProviderAsync - Exception: {0}", ex);
            //await Toast.Make(ex.Message).Show();
        }
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity());
        return authenticatedUser;
    }

    public async Task LogoutAsync()
    {
        await _authenticationService.SignOutAsync();
        _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        _hasCheckedAuthenticationState = true;
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }
}
