using BookShelves.Web.Handlers;
using CommunityToolkit.Datasync.Client.Http;

namespace BookShelves.Web.Services;

internal sealed class BooksDatasyncClientFactory
{
    private readonly HttpClientFactory _factory;
    private readonly ILogger<BooksDatasyncClientFactory> _logger;

    public BooksDatasyncClientFactory(IConfiguration configuration, BearerTokenHandler bearerTokenHandler, ILogger<BooksDatasyncClientFactory> logger)
    {
        _logger = logger;

        var endpoint = configuration["BooksApi:BaseUrl"]
            ?? throw new InvalidOperationException("Missing BooksApi:BaseUrl configuration for Datasync client.");

        if (!endpoint.EndsWith("/"))
        {
            endpoint += "/";
        }

        // 1. Force the combined endpoint to include the tables/ prefix at construction time
        var fullApiUri = new Uri(new Uri(endpoint), "tables/");

        _logger.LogTrace("[DATASYNC DEBUG] Creating BooksDatasyncClientFactory with endpoint: {fullApiUri}", fullApiUri);

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
            Endpoint = fullApiUri,
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
