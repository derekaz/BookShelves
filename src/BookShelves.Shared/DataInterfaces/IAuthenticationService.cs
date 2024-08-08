// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ComponentModel;
using System.Security.Claims;
using Microsoft.Graph;

namespace BookShelves.Shared.DataInterfaces
{
    /// <summary>
    /// Service that provides authentication methods and access to 
    /// an authenticated GraphServiceClient.
    /// </summary>
    public interface IAuthenticationService : INotifyPropertyChanged
    {
        /// <summary>
        /// An authenticated GraphServiceClient for the signed-in user.
        /// </summary>
        public GraphServiceClient GraphClient { get; }
        public ClaimsPrincipal CurrentPrincipal { get; }

        public bool IsSignedIn { get; }

        /// <summary>
        /// Check if a user has signed in.
        /// </summary>
        /// <returns>true if a user has signed in, false if not</returns>
        public Task<bool> IsAuthenticatedAsync();

        /// <summary>
        /// Attempts to sign in a user.
        /// </summary>
        /// <returns>true if authentication succeeds, false if not</returns>
        public Task<bool> SignInAsync();

        /// <summary>
        /// Sign out the user.
        /// </summary>
        public Task SignOutAsync();
    }
}