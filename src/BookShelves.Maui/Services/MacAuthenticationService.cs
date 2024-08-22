using BookShelves.Maui.Models;
using BookShelves.Shared.DataInterfaces;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace BookShelves.Maui.Services;


public static class MacAuthCacheConfig
{
    // App settings
    //public static readonly string[] Scopes = new[] {
    //    "040a74c6-5df7-485c-9133-9d5d4c953717/API.Access", "offline_access" };
    //public const string Authority = "https://login.microsoftonline.com/common";
    //public const string ClientId = "91e2b896-1ef4-4c2e-bd36-e3374926ed77";
    public const string RedirectURI = "bookshelves://callback";
}

public class MacAuthenticationService : IMacAuthenticationService
{
    private readonly ISettingsService _settingsService;
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private TokenResponseModel _token;
    public MacAuthenticationService(ISettingsService? settingsService, HttpClient? httpClient)
    {
        if (settingsService == null) throw new NullReferenceException(nameof(settingsService));
        if (httpClient == null) throw new NullReferenceException(nameof(httpClient));

        _httpClient = httpClient;
        _settingsService = settingsService;

        HttpStatusCode[] httpStatusCodesWorthRetrying = {
               HttpStatusCode.RequestTimeout, // 408
               HttpStatusCode.InternalServerError, // 500
               HttpStatusCode.BadGateway, // 502
               HttpStatusCode.ServiceUnavailable, // 503
               HttpStatusCode.GatewayTimeout // 504
            };
        _retryPolicy = Policy
            .Handle<HttpRequestException>()
            .OrInner<TaskCanceledException>()
            .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
              .WaitAndRetryAsync(new[]
              {
                System.TimeSpan.FromSeconds(2),
                System.TimeSpan.FromSeconds(4),
                System.TimeSpan.FromSeconds(8)
              });
        _token = null;
    }

    public async Task<string> GetTokenAsync(string[] scopes, bool silentOnly)
    {
        try
        {
            await GetTokenFromStorageAsync();
            if (_token != null)
            {
                if (_token.expires_at > DateTime.UtcNow.AddSeconds(30))
                {
                    return _token.access_token;
                }
                return await GetRefreshTokenAsync(scopes, silentOnly);
            }
            else
            {
                return await GetWebTokenAsync(scopes, silentOnly);
            }
        }
        catch (Exception ex)
        {
            return string.Empty;
        }
    }

    public void LogOut()
    {
        _token = null;
        string refreshToken;
#if MACCATALYST
        Preferences.Remove(nameof(refreshToken));
#else
        SecureStorage.Remove(nameof(refreshToken));
#endif
    }

    private async Task GetTokenFromStorageAsync()
    {
        if (_token == null)
        {
            string refreshToken = string.Empty;
#if MACCATALYST
            refreshToken = Preferences.Get(nameof(refreshToken), string.Empty);
#else
            refreshToken = await SecureStorage.GetAsync(nameof(refreshToken));
#endif
            if (!string.IsNullOrEmpty(refreshToken))
            {
                _token = new()
                {
                    refresh_token = refreshToken,
                };
            }
        }
    }

    private async Task SetTokenInStorageAsync(string refreshToken)
    {
#if MACCATALYST
        Preferences.Set(nameof(refreshToken), refreshToken);
#else
        await SecureStorage.SetAsync(nameof(refreshToken), refreshToken);
#endif
    }

    private async Task<string> GetRefreshTokenAsync(string[] scopes, bool silentOnly)
    {
        //https://login.microsoftonline.com/common/oauth2/v2.0/token
        //-ContentType application/x-www-form-urlencoded -Method POST
        //&code=$code&grant_type=refresh_token
        //&refresh_token=$($oauthTokens.refresh_token)"
        List<KeyValuePair<string, string>> loginPayload = new();
        loginPayload.Add(new("client_id", _settingsService.ClientId));
        //loginPayload.Add(new("client_id", MacAuthCacheConfig.ClientId));
        loginPayload.Add(new("scope", string.Join(" ", scopes)));
        loginPayload.Add(new("grant_type", "refresh_token"));
        loginPayload.Add(new("refresh_token", _token.refresh_token));
        _token = await GetTokenFromAzAsync(scopes, loginPayload);
        if (_token != null)
        {
            return _token.access_token;
        }
        return await GetWebTokenAsync(scopes, silentOnly);
    }

    private async Task<string> GetWebTokenAsync(string[] scopes, bool silentOnly)
    {
        if (!silentOnly)
        {
#if WINDOWS
            var result = await WinUIEx.WebAuthenticator.AuthenticateAsync(
                new System.Uri(GenerateCodeUri(scopes)),
                new System.Uri(MacAuthCacheConfig.RedirectURI));
#else
            var result = await WebAuthenticator.AuthenticateAsync(
                new System.Uri(GenerateCodeUri(scopes)),
                new System.Uri(MacAuthCacheConfig.RedirectURI));
#endif
            var code = result.Properties["code"];
            List<KeyValuePair<string, string>> loginPayload = new();
            loginPayload.Add(new("client_id", _settingsService.ClientId));
            //loginPayload.Add(new("client_id", MacAuthCacheConfig.ClientId));
            loginPayload.Add(new("scope", string.Join(" ", scopes)));
            loginPayload.Add(new("grant_type", "authorization_code"));
            loginPayload.Add(new("code", code));
            loginPayload.Add(new("redirect_uri", MacAuthCacheConfig.RedirectURI));
            _token = await GetTokenFromAzAsync(scopes, loginPayload);
            return _token.access_token;
        }
        else
        {
            return string.Empty;
        }
    }

    private async Task<TokenResponseModel> GetTokenFromAzAsync(
        string[] scopes, List<KeyValuePair<string, string>> loginPayload)
    {
        HttpResponseMessage responseMessage = await
            _retryPolicy.ExecuteAsync(async () =>
                         await CreateMessageAndSendAsync(
                             _settingsService.AzureAdAuthority.TrimEnd('/') +
                                 "/oauth2/v2.0/token", loginPayload));
        if (responseMessage.IsSuccessStatusCode)
        {
            string responseText = await responseMessage.Content.ReadAsStringAsync();
            TokenResponseModel tokenResponse = JsonSerializer.Deserialize
                <TokenResponseModel>(responseText);
            if (tokenResponse != null)
            {
                tokenResponse.expires_at = DateTime.UtcNow.AddSeconds(
                    tokenResponse.expires_in);
                await SetTokenInStorageAsync(tokenResponse.refresh_token);
                return tokenResponse;
            }
            throw new Exception("Error getting token");
        }
        else
        {
            string responseText = await responseMessage.Content.ReadAsStringAsync();
            throw new Exception(responseText);
        }
    }

    private async Task<HttpResponseMessage> CreateMessageAndSendAsync(string url,
        List<KeyValuePair<string, string>> payload)
    {
        HttpRequestMessage requestMessage = new(HttpMethod.Post,
            url)
        {
            Content = new FormUrlEncodedContent(payload)
        };
        return await _httpClient.SendAsync(requestMessage);
    }

    private string GenerateCodeUri(string[] scopes)
    {
        return _settingsService.AzureAdAuthority.TrimEnd('/') +
            $"/oauth2/v2.0/authorize?client_id={_settingsService.ClientId}&scope=" +
            //$"/oauth2/v2.0/authorize?client_id={MacAuthCacheConfig.ClientId}&scope=" +
            string.Join(" ", scopes) +
            $"&redirect_uri={MacAuthCacheConfig.RedirectURI}" +
            $"&response_type=code" +
            $"&prompt=login";
    }

}