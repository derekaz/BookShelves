using BookShelves.Maui.Data.SyncTest;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services;
using CommunityToolkit.Datasync.Client.Offline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookShelves.Maui.Data.Infrastructure;

public class SyncUnitOfWork<TContext> : UnitOfWork<TContext>, ISyncUnitOfWork<TContext>
    where TContext : SyncDbContext
{
    private readonly ISyncProgressService _syncProgressService;
    private readonly ILogger<SyncUnitOfWork<TContext>> _logger;

    public SyncUnitOfWork(IDbContextFactory<TContext> contextFactory, ISyncProgressService syncProgressService, ILogger<SyncUnitOfWork<TContext>> logger)
        : base(contextFactory)
    {
        _syncProgressService = syncProgressService;
        _logger = logger;
    }

    public async Task SynchronizeAsync(CancellationToken cancellationToken = default)
    {
        Context.SynchronizationProgress += OnSynchronizationProgress;

        try
        {
            _syncProgressService.ReportStage(SyncStage.Started, "Synchronization started");

            await Context.SynchronizeAsync(cancellationToken);

            _syncProgressService.ReportStage(SyncStage.Completed, "Synchronization complete");
        }
        catch (Exception ex)
        {
            _syncProgressService.ReportStage(SyncStage.Failed, $"Synchronization failed: {ex.Message}");
            throw;
        }
        finally
        {
            Context.SynchronizationProgress -= OnSynchronizationProgress;
        }
    }

    private void OnSynchronizationProgress(object? sender, SynchronizationEventArgs args)
    {
        _logger.LogTrace(
            "Sync progress event: {EventType} ({ItemsProcessed}/{ItemsTotal})",
            args.EventType,
            args.ItemsProcessed,
            args.ItemsTotal);

        var syncStage = args.EventType switch
        {
            SynchronizationEventType.PushStarted or
            SynchronizationEventType.PushItem or
            SynchronizationEventType.PushEnded => SyncStage.Pushing,

            SynchronizationEventType.PullStarted or
            SynchronizationEventType.ItemsFetched or
            SynchronizationEventType.PullEnded => SyncStage.Pulling,

            SynchronizationEventType.ItemsCommitted => SyncStage.Processing,
            SynchronizationEventType.LocalException => SyncStage.Failed,
            _ => SyncStage.None
        };

        var message = args.EventType switch
        {
            SynchronizationEventType.PushStarted => "Pushing local changes",
            SynchronizationEventType.PushItem => "Pushing item",
            SynchronizationEventType.PushEnded => "Finished pushing local changes",
            SynchronizationEventType.PullStarted => "Pulling remote changes",
            SynchronizationEventType.ItemsFetched => "Fetched remote changes",
            SynchronizationEventType.ItemsCommitted => "Committing pulled changes",
            SynchronizationEventType.PullEnded => "Finished pulling remote changes",
            SynchronizationEventType.LocalException => $"Synchronization failed: {args.Exception?.Message}",
            _ => args.EventType.ToString()
        };

        var eventArgs = new SyncProgressEventArgs
        {
            Stage = args.EventType.ToString(),
            Message = message,
            Current = ConvertToInt(args.ItemsProcessed),
            Total = ConvertToInt(args.ItemsTotal),
            SyncStage = syncStage,
            TotalSteps = 3
        };

        _syncProgressService.Report(eventArgs);
    }

    private static int? ConvertToInt(long value)
        => value >= int.MinValue && value <= int.MaxValue ? (int)value : null;
}
