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

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            _logger.LogDebug("Request to {RequestUri} completed with status code {StatusCode}", request.RequestUri, response.StatusCode);
            return response;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed for {RequestUri}", request.RequestUri);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected exception during request pipeline for {RequestUri}", request.RequestUri);
            throw;
        }
    }
}
