using BookShelves.Web.Shared;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;

namespace BookShelves.Web.Services;

public class ServerTokenService : ITokenService
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ServerTokenService> _logger;

    public ServerTokenService(
        ITokenAcquisition tokenAcquisition,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ServerTokenService> logger)
    {
        _tokenAcquisition = tokenAcquisition;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<string?> GetIdTokenAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("User is not authenticated, cannot retrieve ID token");
            return null;
        }

        try
        {
            // First, try to get from claims (added by OnTokenValidated)
            var idTokenFromClaim = httpContext.User.FindFirst("id_token")?.Value
                                ?? httpContext.User.FindFirst("idtoken")?.Value;

            if (!string.IsNullOrEmpty(idTokenFromClaim))
            {
                _logger.LogDebug("Retrieved ID token from user claims");
                return idTokenFromClaim;
            }

            // Fallback: try to get from authentication properties
            var authenticateResult = await httpContext.AuthenticateAsync();
            if (authenticateResult.Succeeded && authenticateResult.Properties != null)
            {
                var idTokenFromProps = authenticateResult.Properties.GetTokenValue("id_token");
                if (!string.IsNullOrEmpty(idTokenFromProps))
                {
                    _logger.LogDebug("Retrieved ID token from authentication properties");
                    return idTokenFromProps;
                }
            }

            _logger.LogWarning("ID token not found in claims or authentication properties. Ensure SaveTokens = true and OnTokenValidated adds token claims.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ID token");
            return null;
        }
    }

    public async Task<string?> GetAccessTokenAsync(string[]? scopes = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            _logger.LogDebug("User is not authenticated, cannot retrieve access token");
            return null;
        }

        try
        {
            // If scopes are specified, use ITokenAcquisition to get a fresh token
            if (scopes != null && scopes.Length > 0)
            {
                _logger.LogDebug("Acquiring access token for scopes: {Scopes}", string.Join(", ", scopes));
                var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
                return token;
            }

            // Otherwise, try to get from claims first
            var accessTokenFromClaim = httpContext.User.FindFirst("access_token")?.Value
                                    ?? httpContext.User.FindFirst("accesstoken")?.Value;

            if (!string.IsNullOrEmpty(accessTokenFromClaim))
            {
                _logger.LogDebug("Retrieved access token from user claims");
                return accessTokenFromClaim;
            }

            // Fallback: try authentication properties
            var authenticateResult = await httpContext.AuthenticateAsync();
            if (authenticateResult.Succeeded && authenticateResult.Properties != null)
            {
                var accessTokenFromProps = authenticateResult.Properties.GetTokenValue("access_token");
                if (!string.IsNullOrEmpty(accessTokenFromProps))
                {
                    _logger.LogDebug("Retrieved access token from authentication properties");
                    return accessTokenFromProps;
                }
            }

            // Last resort: try to acquire with default scopes
            _logger.LogDebug("Access token not found in claims/properties, attempting acquisition with empty scopes");
            var acquiredToken = await _tokenAcquisition.GetAccessTokenForUserAsync(Array.Empty<string>());
            return acquiredToken;
        }
        catch (MicrosoftIdentityWebChallengeUserException)
        {
            // User needs to consent or re-authenticate
            _logger.LogWarning("User challenge required for access token acquisition");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving access token");
            return null;
        }
    }
}
