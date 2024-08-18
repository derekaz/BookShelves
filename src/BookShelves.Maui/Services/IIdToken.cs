//using BookShelves.Maui.Services;
//using Microsoft.Graph;
//using Microsoft.Identity.Client;
//using Microsoft.Kiota.Abstractions.Authentication;
//using Microsoft.Kiota.Abstractions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using Microsoft.Graph;
using Microsoft.Kiota.Abstractions;
using System.Security.Claims;

namespace BookShelves.Maui.Services;

public interface IIdToken
{
    ClaimsPrincipal CurrentPrincipal { get; }
    GraphServiceClient GraphClient { get; }
    bool IsSignedIn { get; }

    Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object> additionalAuthenticationContext = null, CancellationToken cancellationToken = default);
    Task<bool> IsAuthenticatedAsync();
    Task<bool> SignInAsync();
    Task SignOutAsync();
}