#define OFFLINE_SYNC_ENABLED

using CommunityToolkit.Datasync.Client.Http;
using CommunityToolkit.Datasync.Client.Offline;
using Microsoft.EntityFrameworkCore;

namespace BookShelves.Maui.Data.SyncTest;

#if OFFLINE_SYNC_ENABLED
public class AuthorDbContext(DbContextOptions<AuthorDbContext> options) : OfflineDbContext(options)
#else
public class AuthorDbContext(DbContextOptions<AuthorDbContext> options) : DbContext(options)
#endif
{
    public DbSet<AuthorItem> AuthorItems => Set<AuthorItem>();

#if OFFLINE_SYNC_ENABLED
    protected override void OnDatasyncInitialization(DatasyncOfflineOptionsBuilder optionsBuilder)
    {
        HttpClientOptions clientOptions = new()
        {
            Endpoint = new Uri("https://localhost:7135"),
            HttpPipeline = [new LoggingHandler()]
        };
        _ = optionsBuilder.UseHttpClientOptions(clientOptions);
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