using Microsoft.Identity.Web;

namespace BookShelves.Web.Services;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly ITokenAcquisition _tokenService; // Inject your token acquisition service here

    public BearerTokenHandler(ITokenAcquisition tokenService)
    {
        _tokenService = tokenService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Acquire the token (adapt this to your specific token service/logic)
        string[] scopes = ["api://a98249d2-b51b-41d6-9c2a-5dadf7cf276f/Books.ReadWrite"];

        var token = await _tokenService.GetAccessTokenForUserAsync(scopes);

        // var token = await _tokenService.GetAccessTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            // Set the Authorization header directly
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}