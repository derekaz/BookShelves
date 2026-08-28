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

    public static void UseTestUserId(this HttpClient client, string userId)
    {
        if (client.DefaultRequestHeaders.Contains("X-Test-UserId"))
        {
            client.DefaultRequestHeaders.Remove("X-Test-UserId");
        }

        client.DefaultRequestHeaders.Add("X-Test-UserId", userId);
    }

    public static void UseTestRoles(this HttpClient client, params string[] roles)
    {
        if (client.DefaultRequestHeaders.Contains("X-Test-Roles"))
        {
            client.DefaultRequestHeaders.Remove("X-Test-Roles");
        }

        client.DefaultRequestHeaders.Add("X-Test-Roles", string.Join(',', roles));
    }
}
