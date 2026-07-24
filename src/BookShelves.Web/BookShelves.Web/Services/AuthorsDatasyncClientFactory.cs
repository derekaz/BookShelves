using CommunityToolkit.Datasync.Client.Http;

namespace BookShelves.Web.Services;

internal sealed class AuthorsDatasyncClientFactory
{
    private readonly HttpClientFactory _factory;
    private readonly ILogger<AuthorsDatasyncClientFactory> _logger;

    public AuthorsDatasyncClientFactory(IConfiguration configuration, BearerTokenHandler bearerTokenHandler, ILogger<AuthorsDatasyncClientFactory> logger)
    {
        _logger = logger;

        var endpoint = configuration["BooksApi:BaseUrl"]
            ?? throw new InvalidOperationException("Missing BooksApi:BaseUrl configuration for Datasync client.");

        _logger.LogTrace("[DATASYNC DEBUG] Creating AuthorsDatasyncClientFactory with endpoint: {Endpoint}", endpoint);

        var customHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                // Allow the connection if the cert is completely valid OR if it just has a name mismatch
                return errors == System.Net.Security.SslPolicyErrors.None ||
                       errors == System.Net.Security.SslPolicyErrors.RemoteCertificateNameMismatch;
            }
        };

        HttpClientOptions options = new()
        {
            Endpoint = new Uri(endpoint),
            HttpPipeline =
            [
                bearerTokenHandler,
                customHandler
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
