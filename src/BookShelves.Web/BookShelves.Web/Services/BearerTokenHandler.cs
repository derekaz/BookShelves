using Microsoft.Identity.Web;

namespace BookShelves.Web.Services;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly ITokenAcquisition _tokenService; // Inject your token acquisition service here
    private readonly ILogger<BearerTokenHandler> _logger;

    public BearerTokenHandler(ITokenAcquisition tokenService, ILogger<BearerTokenHandler> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
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

        _logger.LogTrace($"[DATASYNC DEBUG] Outgoing Request URL: {request.RequestUri}");

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogTrace($"[DATASYNC DEBUG] Response Status Code: {response.StatusCode}");

        if (!response.IsSuccessStatusCode && response.Content != null)
        {
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogTrace($"[DATASYNC DEBUG] Response Body Content:\n{content}");
        }

        return response;
    }
}