using BookShelves.Maui.Data.Interfaces;
using BookShelves.Maui.Data.Models;
using CommunityToolkit.Datasync.Client.Offline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookShelves.Maui.Data.Infrastructure;

public class SyncDbContext : OfflineDbContext
{
    private readonly ILogger<SyncDbContext> _logger;
    private readonly ISyncApiClient _syncApiClient;

    public SyncDbContext(DbContextOptions<SyncDbContext> options, ILogger<SyncDbContext> logger, ISyncApiClient syncApiClient)
        : base(options)
    {
        _logger = logger;
        _syncApiClient = syncApiClient;
    }

    public DbSet<Author> AuthorItems => Set<Author>();
    public DbSet<Book> BookItems => Set<Book>();

    protected override void OnDatasyncInitialization(DatasyncOfflineOptionsBuilder optionsBuilder)
    {
        optionsBuilder.Entity<Author>(cfg =>
        {
            cfg.Endpoint = new Uri("tables/Authors", UriKind.Relative);
        });

        optionsBuilder.Entity<Book>(cfg =>
        {
            cfg.Endpoint = new Uri("tables/Books", UriKind.Relative);
        });

        _ = optionsBuilder.UseHttpClient(_syncApiClient.HttpClient);
    }

    public virtual async Task SynchronizeAsync(CancellationToken cancellationToken = default)
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