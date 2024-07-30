using Azure.Core;
using BookShelves.Maui.Services;
using BookShelves.Shared.DataInterfaces;
using CommunityToolkit.Maui.Alerts;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Client;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BookShelves.Maui.Helpers;

public class ExternalAuthenticationStateProvider : AuthenticationStateProvider, IExternalAuthenticationStateProvider
{
    //public IPublicClientApplication IdentityClient { get; set; }
    private ClaimsPrincipal currentUser = new ClaimsPrincipal(new ClaimsIdentity());
    //private string? accessToken = null;
    private IAuthenticationService _authenticationService;
    //private AuthService? authService = null;

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(currentUser));
    }

    public ExternalAuthenticationStateProvider(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
        //authService = new AuthService();
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
            //if (authService == null) { throw new InvalidOperationException("AuthenticationService not defined"); }

            var result = await _authenticationService.SignInAsync();
            if (result) return _authenticationService.CurrentPrincipal;

            //_authenticationService.GetUserAccount
            
            //var result = await authService.LoginAsync(CancellationToken.None);
            //var token = result?.IdToken;
            //accessToken = result?.AccessToken;
            ////JwtSecurityToken data = null;
            //if (token != null)
            //{
            //    var handler = new JwtSecurityTokenHandler();
            //    var data = handler.ReadJwtToken(token);
                
            //    return new ClaimsPrincipal(new ClaimsIdentity(data.Claims, "TEST"));
            //}
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
        //await authService.LogoutAsync();
        currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(currentUser)));
    }
}
