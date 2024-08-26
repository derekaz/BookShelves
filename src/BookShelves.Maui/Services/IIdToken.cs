//using Microsoft.Graph;
//using Microsoft.Kiota.Abstractions;
//using System.Security.Claims;

//namespace BookShelves.Maui.Services;

//public interface IIdToken
//{
//    ClaimsPrincipal CurrentPrincipal { get; }
//    GraphServiceClient GraphClient { get; }
//    bool IsSignedIn { get; }

//    Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object> additionalAuthenticationContext = null, CancellationToken cancellationToken = default);
//    Task<bool> IsAuthenticatedAsync();
//    Task<bool> SignInAsync();
//    Task SignOutAsync();
//}