using BookShelves.Web.Handlers;
using CommunityToolkit.Datasync.Client.Http;

namespace BookShelves.Web.Services;

internal sealed class BookUserActionsDatasyncClientFactory
{
    private readonly HttpClientFactory _factory;
    private readonly ILogger<BookUserActionsDatasyncClientFactory> _logger;

    public BookUserActionsDatasyncClientFactory(IConfiguration configuration, BearerTokenHandler bearerTokenHandler, ILogger<BookUserActionsDatasyncClientFactory> logger)
    {
        _logger = logger;

        var endpoint = configuration["BooksApi:BaseUrl"]
            ?? throw new InvalidOperationException("Missing BooksApi:BaseUrl configuration for Datasync client.");

        if (!endpoint.EndsWith("/"))
        {
            endpoint += "/";
        }

        var fullApiUri = new Uri(new Uri(endpoint), "tables/");

        _logger.LogTrace("[DATASYNC DEBUG] Creating BookUserActionsDatasyncClientFactory with endpoint: {fullApiUri}", fullApiUri);

        var customHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
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
