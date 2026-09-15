using Microsoft.Identity.Web;

namespace BookShelves.Web.Handlers;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly ITokenAcquisition _tokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BearerTokenHandler> _logger;

    public BearerTokenHandler(ITokenAcquisition tokenService, IConfiguration configuration, ILogger<BearerTokenHandler> logger)
    {
        _tokenService = tokenService;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var scopes = _configuration.GetSection("BooksApi:Scopes").Get<string[]>()
            ?? ["api://a98249d2-b51b-41d6-9c2a-5dadf7cf276f/Books.ReadWrite"];

        var token = await _tokenService.GetAccessTokenForUserAsync(scopes);

        if (!string.IsNullOrWhiteSpace(token))
        {
            // Set the Authorization header directly
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            _logger.LogTrace($"[DATASYNC DEBUG] Outgoing Request BearerToken: {token}");
        }

        _logger.LogTrace($"[DATASYNC DEBUG] Outgoing Request URL: {request.RequestUri}");

        var response = await base.SendAsync(request, cancellationToken);

        _logger.LogTrace($"[DATASYNC DEBUG] Response Status Code: {response.StatusCode}");

        //if (!response.IsSuccessStatusCode && response.Content != null)
        if (response.Content != null)
        {
            _logger.LogTrace($"[DATASYNC DEBUG] Outgoing Request URL: {request.RequestUri}");
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogTrace($"[DATASYNC DEBUG] Response Body Content:\n{content}");
        }

        return response;
    }
}