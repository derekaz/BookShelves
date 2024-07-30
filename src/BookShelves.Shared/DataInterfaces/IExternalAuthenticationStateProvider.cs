using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Client;

namespace BookShelves.Shared.DataInterfaces;

public interface IExternalAuthenticationStateProvider
{
    //IPublicClientApplication IdentityClient { get; set; }

    Task<AuthenticationState> GetAuthenticationStateAsync();
    Task LogInAsync();
    Task LogoutAsync();
}