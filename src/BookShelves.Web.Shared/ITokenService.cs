namespace BookShelves.Web.Shared;

/// <summary>
/// Provides access to authentication tokens across different platforms and render modes.
/// Implementations exist for server-side (Web) and client-side (Web.Client) scenarios.
/// </summary>
public interface ITokenService
{
	/// <summary>
	/// Gets the OpenID Connect ID token for the authenticated user.
	/// The ID token contains user identity claims and is used for authentication verification.
	/// </summary>
	/// <returns>
	/// The ID token as a JWT string, or null if:
	/// - User is not authenticated
	/// - Token was not captured during authentication
	/// - Token has expired or been cleared
	/// </returns>
	/// <remarks>
	/// <para><strong>Server-side (ServerTokenService):</strong></para>
	/// <list type="bullet">
	///   <item>Reads from user claims (added by OnTokenValidated)</item>
	///   <item>Falls back to authentication properties if SaveTokens is enabled</item>
	/// </list>
	/// 
	/// <para><strong>Client-side (ClientTokenService):</strong></para>
	/// <list type="bullet">
	///   <item>Reads from UserInfo in PersistentComponentState</item>
	///   <item>Falls back to authentication state claims</item>
	/// </list>
	/// 
	/// <para><strong>Usage example:</strong></para>
	/// <code>
	/// @inject ITokenService TokenService
	/// 
	/// var idToken = await TokenService.GetIdTokenAsync();
	/// if (!string.IsNullOrEmpty(idToken))
	/// {
	///     // Use token for identity verification
	///     var handler = new JwtSecurityTokenHandler();
	///     var jwt = handler.ReadJwtToken(idToken);
	///     var claims = jwt.Claims;
	/// }
	/// </code>
	/// </remarks>
	Task<string?> GetIdTokenAsync();

	/// <summary>
	/// Gets an OAuth 2.0 access token for the authenticated user.
	/// The access token is used to authorize API requests.
	/// </summary>
	/// <param name="scopes">
	/// Optional array of OAuth scopes to request. 
	/// - If provided (server-side only), will attempt to acquire a new token with those scopes via ITokenAcquisition.
	/// - If null or empty, returns the cached token or attempts acquisition with default scopes.
	/// - On client-side, this parameter is ignored as client cannot request new tokens.
	/// </param>
	/// <returns>
	/// The access token string, or null if:
	/// - User is not authenticated
	/// - Token was not captured during authentication
	/// - Token has expired and cannot be refreshed
	/// - Requested scopes require user consent (throws MicrosoftIdentityWebChallengeUserException)
	/// </returns>
	/// <remarks>
	/// <para><strong>Server-side (ServerTokenService):</strong></para>
	/// <list type="bullet">
	///   <item>If scopes are specified: Uses ITokenAcquisition to get a token for those scopes</item>
	///   <item>Otherwise: Reads from user claims first</item>
	///   <item>Falls back to authentication properties</item>
	///   <item>Final fallback: ITokenAcquisition with empty scopes</item>
	/// </list>
	/// 
	/// <para><strong>Client-side (ClientTokenService):</strong></para>
	/// <list type="bullet">
	///   <item>Reads from UserInfo in PersistentComponentState</item>
	///   <item>Checks token expiration before returning</item>
	///   <item>Falls back to authentication state claims</item>
	///   <item>Cannot acquire new tokens with scopes (client limitation)</item>
	/// </list>
	/// 
	/// <para><strong>Usage example for API calls:</strong></para>
	/// <code>
	/// @inject ITokenService TokenService
	/// @inject HttpClient Http
	/// 
	/// var accessToken = await TokenService.GetAccessTokenAsync();
	/// 
	/// var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");
	/// request.Headers.Authorization = 
	///     new AuthenticationHeaderValue("Bearer", accessToken);
	/// 
	/// var response = await Http.SendAsync(request);
	/// </code>
	/// 
	/// <para><strong>Usage example with specific scopes (server-side only):</strong></para>
	/// <code>
	/// var token = await TokenService.GetAccessTokenAsync(new[] 
	/// { 
	///     "https://graph.microsoft.com/.default" 
	/// });
	/// </code>
	/// </remarks>
	/// <exception cref="MicrosoftIdentityWebChallengeUserException">
	/// Thrown when the user needs to provide consent for the requested scopes.
	/// This typically requires redirecting the user to re-authenticate with additional permissions.
	/// </exception>
	Task<string?> GetAccessTokenAsync(string[]? scopes = null);
}

/// <summary>
/// Implementation guidance:
/// 
/// <para><strong>For Server-Side (TestMauiBlazorWebApp1.Web):</strong></para>
/// <list type="number">
///   <item>Implement as ServerTokenService</item>
///   <item>Inject ITokenAcquisition and IHttpContextAccessor</item>
///   <item>Use multi-source fallback: claims → auth properties → ITokenAcquisition</item>
///   <item>Register as: builder.Services.AddScoped&lt;ITokenService, ServerTokenService&gt;()</item>
/// </list>
/// 
/// <para><strong>For Client-Side (TestMauiBlazorWebApp1.Web.Client):</strong></para>
/// <list type="number">
///   <item>Implement as ClientTokenService</item>
///   <item>Inject PersistentComponentState and AuthenticationStateProvider</item>
///   <item>Read from persisted UserInfo, fallback to auth state claims</item>
///   <item>Register as: builder.Services.AddScoped&lt;ITokenService, ClientTokenService&gt;()</item>
/// </list>
/// 
/// <para><strong>Token Flow Requirements:</strong></para>
/// <list type="number">
///   <item>Web\Program.cs must configure OpenIdConnect with SaveTokens = true</item>
///   <item>OnTokenValidated event must add tokens as claims to the identity</item>
///   <item>PersistingServerAuthenticationStateProvider must serialize tokens to UserInfo</item>
///   <item>PersistentAuthenticationStateProvider must deserialize UserInfo and recreate claims</item>
/// </list>
/// 
/// <para><strong>Security Considerations:</strong></para>
/// <list type="bullet">
///   <item>Tokens are sensitive - never log full token values in production</item>
///   <item>Tokens are kept in memory only, not persisted to localStorage</item>
///   <item>ID tokens are for identity verification only, not API authorization</item>
///   <item>Access tokens should have short lifetimes and be refreshed as needed</item>
///   <item>Consider whether refresh token persistence is necessary for your scenario</item>
/// </list>
/// </summary>
