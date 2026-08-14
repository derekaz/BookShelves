using System.Net.Http.Headers;

namespace BookShelves.WebApi.Tests.TestUtilities;

internal static class HttpClientAuthExtensions
{
    public static void UseTestBearerToken(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
    }

    public static void UseTestScopes(this HttpClient client, string scopes)
    {
        if (client.DefaultRequestHeaders.Contains("X-Test-Scopes"))
        {
            client.DefaultRequestHeaders.Remove("X-Test-Scopes");
        }

        client.DefaultRequestHeaders.Add("X-Test-Scopes", scopes);
    }
}
