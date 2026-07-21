using CommunityToolkit.Datasync.Client.Offline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookShelves.Maui.Data.SyncTest;

public class SyncDbContext : OfflineDbContext
{
    private readonly ILogger<SyncDbContext> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public SyncDbContext(DbContextOptions<SyncDbContext> options, ILogger<SyncDbContext> logger, IHttpClientFactory httpClientFactory)
        : base(options)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public DbSet<Author> AuthorItems => Set<Author>();

    protected override void OnDatasyncInitialization(DatasyncOfflineOptionsBuilder optionsBuilder)
    {
        // Resolve your pre-configured client here
        var httpClient = _httpClientFactory.CreateClient("SyncApi");

        optionsBuilder.Entity<Author>(cfg =>
        {
            cfg.Endpoint = new Uri("Authors", UriKind.Relative);
        });

        _ = optionsBuilder.UseHttpClient(httpClient);
    }

    public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Starting synchronization...");
        _logger.LogTrace("Pushing local changes to the server...");
        PushResult pushResult = await this.PushAsync(cancellationToken);
        if (!pushResult.IsSuccessful)
        {
            throw new ApplicationException($"Push failed: {pushResult.FailedRequests.FirstOrDefault().Value.ReasonPhrase}");
        }

        _logger.LogTrace("Pulling remote changes from the server...");
        PullResult pullResult = await this.PullAsync(cancellationToken);
        if (!pullResult.IsSuccessful)
        {
            throw new ApplicationException($"Pull failed: {pullResult.FailedRequests.FirstOrDefault().Value.ReasonPhrase}");
        }
        _logger.LogTrace("Completed synchronization...");
    }
}