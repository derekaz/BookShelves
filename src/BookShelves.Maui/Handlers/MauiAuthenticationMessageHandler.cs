using BookShelves.Maui.Services;
using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

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
                _logger.LogDebug("Added Bearer token to request for {RequestUri}", request.RequestUri);
            }
            else
            {
                _logger.LogWarning("No access token available for request to {RequestUri}", request.RequestUri);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access token for request to {RequestUri}", request.RequestUri);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
