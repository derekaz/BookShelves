using BookShelves.Shared.DataInterfaces;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace BookShelves.WasmSwa.Services
{
    public class GraphService : IGraphService
    {
        private readonly GraphServiceClient _graphServiceClient;

        private User? _user;
        private Stream? _userPhoto;

        public GraphService(GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public async Task<User?> GetUserInfoAsync()
        {
            var graphClient = _graphServiceClient;

            //if (_authenticationService.IsSignedIn)
            //{
                if (_user == null)
                {
                    // Get the user, cache for subsequent calls
                    _user = await graphClient.Me.GetAsync(
                        requestConfiguration =>
                        {
                            requestConfiguration.QueryParameters.Select =
                                new[] {
                                    "displayName", 
                                    //"mail", 
                                    //"mailboxSettings", 
                                    "userPrincipalName"
                                };
                        });
                }
            //}
            //else
            //{
            //    _user = null;
            //}

            return _user;
        }

        public async Task<Stream?> GetUserPhotoAsync()
        {
            var graphClient = _graphServiceClient;

            //if (_authenticationService.IsSignedIn)
            //{
                if (_userPhoto == null)
                {
                    // Get the user photo, cache for subsequent calls
                    _userPhoto = await graphClient.Me
                        .Photo
                        .Content
                        .GetAsync();
                }
            //}
            //else
            //{
            //    _userPhoto = null;
            //}

            return _userPhoto;
        }
    }
}
