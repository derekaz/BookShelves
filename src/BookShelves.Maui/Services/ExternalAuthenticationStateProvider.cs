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
    // private AuthenticationState _currentAuthenticationState = new AuthenticationState(new ClaimsPrincipal());
    private readonly IAuthenticationService _authenticationService = authenticationService;
    private readonly ILogger<ExternalAuthenticationStateProvider> _logger = logger;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var temp = await Task.FromResult(new AuthenticationState(_currentUser));
        // var temp = await Task.FromResult(_currentAuthenticationState);
        return temp;
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
        var loginTask = await _authenticationService.IsAuthenticatedAsync();
        if (loginTask)
        {
            _currentUser = _authenticationService.CurrentPrincipal;
            var state = Task.FromResult(new AuthenticationState(_currentUser));
            NotifyAuthenticationStateChanged(state);
            await state;
        }
        return;
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

    //public Task LogInAsync()
    //{
    //    var loginTask = LogInAsyncCore();

    //    return loginTask;
    //}

    //private async Task<AuthenticationState> LogInAsyncCore()
    //{
    //    var user = await LoginWithExternalProviderAsync();
    //    _currentUser = user;
    //    var state = Task.FromResult(new AuthenticationState(_currentUser));
    //    NotifyAuthenticationStateChanged(state);
    //    return state.Result;
    //}

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
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
    }

    //public void NotifyUserLoggedIn(ClaimsPrincipal principal)
    //{
    //    _currentAuthenticationState = new AuthenticationState(principal);
    //    NotifyAuthenticationStateChanged(Task.FromResult(_currentAuthenticationState));
    //}
}
