using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.AspNetCore.Components;

namespace BookShelves.Web.Services
{
    public class AuthService(IServiceProvider serviceProvider) : IAuthService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

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
            navigationManager.NavigateTo("authentication/logout"); // , "/");
        }
    }
}
