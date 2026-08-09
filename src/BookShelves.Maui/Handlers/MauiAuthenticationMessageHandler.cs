using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace BookShelves.Maui.Handlers;

internal class MauiAuthenticationMessageHandler : DelegatingHandler
{
    private readonly IExternalAuthenticationStateProvider _authenticationStateProvider;
    private readonly ILogger<MauiAuthenticationMessageHandler> _logger;
    private readonly string[] _scopes;

    public MauiAuthenticationMessageHandler(
        IExternalAuthenticationStateProvider authenticationStateProvider,
        ILogger<MauiAuthenticationMessageHandler> logger,
        string[] scopes)
    {
        _authenticationStateProvider = authenticationStateProvider ?? throw new ArgumentNullException(nameof(authenticationStateProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            var token = await _authenticationStateProvider.GetAccessTokenAsync(_scopes);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("Added Bearer token to request for {RequestUri} using scopes {Scopes}", request.RequestUri, _scopes);
            }
            else
            {
                _logger.LogWarning("No access token available for request to {RequestUri} using scopes {Scopes}", request.RequestUri, _scopes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access token for request to {RequestUri}", request.RequestUri);
        }

        // --- DIAGNOSTICS LOGGING START ---
        try
        {
            _logger.LogInformation("[DIAG] Outbound Headers for {Uri}: {Headers}", request.RequestUri, request.Headers.ToString());

            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation("[DIAG] Response received. Status: {StatusCode}", response.StatusCode);
            return response;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "[DIAG] HttpRequestException caught hitting {Uri}.", request.RequestUri);

            // Unroll the inner exceptions to catch native iOS error structures
            var inner = httpEx.InnerException;
            int depth = 1;
            while (inner != null)
            {
                _logger.LogError("[DIAG] Inner Exception Level {Depth}: {Type} - {Message}", depth, inner.GetType().Name, inner.Message);

                // Check for specific native WebExceptions or SocketExceptions
                if (inner is System.Net.WebException webEx)
                {
                    _logger.LogError("[DIAG] WebException Status: {Status}", webEx.Status);
                    if (webEx.Response != null)
                    {
                        _logger.LogError("[DIAG] WebException has a response object present.");
                    }
                }

                inner = inner.InnerException;
                depth++;
            }

            throw; // Re-throw to maintain original application behavior
        }
        catch (Exception generalEx)
        {
            _logger.LogError(generalEx, "[DIAG] Non-HTTP Exception caught in pipeline: {Message}", generalEx.Message);
            throw;
        }
        // --- DIAGNOSTICS LOGGING END ---
    }
}
