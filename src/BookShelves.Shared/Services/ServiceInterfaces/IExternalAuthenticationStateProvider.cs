using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Client;

namespace BookShelves.Shared.Services.ServiceInterfaces;

public interface IExternalAuthenticationStateProvider
{
    Task<AuthenticationState> GetAuthenticationStateAsync();
    Task InitializeAsync();
    Task LogInAsync();
    Task LogoutAsync();
}