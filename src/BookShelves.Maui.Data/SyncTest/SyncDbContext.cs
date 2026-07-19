#define OFFLINE_SYNC_ENABLED

using CommunityToolkit.Datasync.Client.Offline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookShelves.Maui.Data.SyncTest;

#if OFFLINE_SYNC_ENABLED
public class SyncDbContext(DbContextOptions<SyncDbContext> options, ILogger<SyncDbContext> logger, IHttpClientFactory httpClientFactory) : OfflineDbContext(options)
#else
public class SyncDbContext(DbContextOptions<SyncDbContext> options, ILogger<SyncDbContext> logger, IHttpClientFactory httpClientFactory) : DbContext(options)
#endif
{
    public DbSet<Author> AuthorItems => Set<Author>();

#if OFFLINE_SYNC_ENABLED
    protected override void OnDatasyncInitialization(DatasyncOfflineOptionsBuilder optionsBuilder)
    {
        // Resolve your pre-configured client here
        var httpClient = httpClientFactory.CreateClient("SyncApi");

        // When using UseHttpClient(), the endpoint is relative to the HttpClient's BaseAddress
        // BaseAddress (from config): https://localhost:7135
        // Endpoint (relative): Authors
        // Final URL: https://localhost:7135/Authors
        optionsBuilder.Entity<Author>(cfg =>
        {
            // Use relative endpoint (without leading slash) that will be appended to BaseAddress
            cfg.Endpoint = new Uri("Authors", UriKind.Relative);

            // Optional: Add query filters
            // cfg.Query.Where(x => x.IsDeleted != true);
        });

        // Use the pre-configured HttpClient from the factory
        _ = optionsBuilder.UseHttpClient(httpClient);
    }
#endif

    public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
#if OFFLINE_SYNC_ENABLED
        PushResult pushResult = await this.PushAsync(cancellationToken);
        if (!pushResult.IsSuccessful)
        {
            throw new ApplicationException($"Push failed: {pushResult.FailedRequests.FirstOrDefault().Value.ReasonPhrase}");
        }

        PullResult pullResult = await this.PullAsync(cancellationToken);
        if (!pullResult.IsSuccessful)
        {
            throw new ApplicationException($"Pull failed: {pullResult.FailedRequests.FirstOrDefault().Value.ReasonPhrase}");
        }
#endif
    }
}