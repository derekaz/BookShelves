using Azure.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;

namespace BookShelves.Shared
{
    internal class GraphService
    {
        private readonly string[] _scopes = new[] { "User.Read" };
        //private const string TenantId = "<add your tenant id here>";
        //private const string ClientId = "<add your client id here>";
        private const string ApplicationId = "1429bc60-21a6-4f87-98a5-27016b33f86a";
        private const string TenantId = "d23695ff-4843-44ec-8c6f-af2de0c2ccc8";
        private GraphServiceClient _client;

        public GraphService()
        {
            Initialize();
        }

        private void Initialize()
        {
            // assume Windows for this sample
            if (OperatingSystem.IsWindows())
            {
                var options = new InteractiveBrowserCredentialOptions
                {
                    TenantId = TenantId,
                    ClientId = ApplicationId,
                    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
                    RedirectUri = new Uri("http://localhost"),
                };
                
                InteractiveBrowserCredential interactiveCredential = new(options);
                _client = new GraphServiceClient(interactiveCredential, _scopes);

                //_client = new GraphServiceClient();

            }
            else
            {
                // TODO: Add iOS/Android support
            }
        }

        public async Task<User> GetMyDetailsAsync()
        {
            try
            {
                return await _client.Me.GetAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user details: {ex}");
                return null;
            }
        }

    }
}
