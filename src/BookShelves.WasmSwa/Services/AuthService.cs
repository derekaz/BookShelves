using BookShelves.Shared.DataInterfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BookShelves.WasmSwa.Services
{
    public class AuthService : IAuthService
    {
        private IServiceProvider _serviceProvider;

        public AuthService(IServiceProvider serviceProvider) 
        { 
            _serviceProvider = serviceProvider;
        }

        public async Task LoginAsync()
        {
            BeginLogin();
        }

        public async Task LogoutAsync()
        {
            BeginLogout();
        }

        private void BeginLogin()
        {
            var navigationManager = _serviceProvider.GetRequiredService<NavigationManager>();
            navigationManager.NavigateTo("authentication/login");
        }

        private void BeginLogout()
        {
            //var signOutManager = _serviceProvider.GetRequiredService<SignOutSessionStateManager>();
            //await SignOutManager.SetSignOutState();
            var navigationManager = _serviceProvider.GetRequiredService<NavigationManager>();
            navigationManager.NavigateToLogout("authentication/logout");
        }
    }
}
