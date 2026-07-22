using CommunityToolkit.Datasync.Client.Http;

namespace BookShelves.Web.Services;

internal sealed class AuthorsDatasyncClientFactory
{
    private readonly HttpClientFactory _factory;

    public AuthorsDatasyncClientFactory(IConfiguration configuration, BearerTokenHandler bearerTokenHandler)
    {
        var endpoint = configuration["BooksApi:BaseUrl"]
            ?? throw new InvalidOperationException("Missing BooksApi:BaseUrl configuration for Datasync client.");

        HttpClientOptions options = new()
        {
            Endpoint = new Uri(endpoint),
            HttpPipeline =
            [
                bearerTokenHandler
            ],
            Timeout = TimeSpan.FromSeconds(120)
        };

        _factory = new HttpClientFactory(options);
    }

    public HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }
}
