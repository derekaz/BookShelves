using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.Extensions.Logging;

namespace BookShelves.Shared.Services;

public sealed class PageSyncCoordinator : IPageSyncCoordinator
{
    private static readonly TimeSpan AutoSyncDelay = TimeSpan.FromSeconds(2);

    private readonly ISyncDataService _syncDataService;
    private readonly ISyncProgressService _syncProgressService;
    private readonly ILogger<PageSyncCoordinator> _logger;
    private readonly SemaphoreSlim _syncOperationGate = new(1, 1);

    private CancellationTokenSource? _autoSyncDelayCts;
    private bool _syncRequested;

    public PageSyncCoordinator(
        ISyncDataService syncDataService,
        ISyncProgressService syncProgressService,
        ILogger<PageSyncCoordinator> logger)
    {
        _syncDataService = syncDataService;
        _syncProgressService = syncProgressService;
        _logger = logger;
    }

    public bool IsSupported => _syncDataService.SupportsSync;

    public Task QueueAutomaticSyncAsync(
        Func<Task> syncAction,
        Func<Task> refreshAction,
        Action<string>? setMessage = null,
        Func<Task>? refreshUi = null,
        bool isAutomaticTrigger = true)
    {
        if (!IsSupported)
        {
            return Task.CompletedTask;
        }

        _syncRequested = true;
        var pendingMessage = "Local changes detected. Waiting to sync...";
        _syncProgressService.ReportStage(SyncStage.Pending, pendingMessage);
        setMessage?.Invoke(pendingMessage);
        _ = refreshUi?.Invoke();

        _autoSyncDelayCts?.Cancel();
        _autoSyncDelayCts?.Dispose();
        _autoSyncDelayCts = new CancellationTokenSource();
        var token = _autoSyncDelayCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(AutoSyncDelay, token);
                await TriggerSyncAsync(syncAction, refreshAction, setMessage, refreshUi, isAutomaticTrigger: true);
            }
            catch (OperationCanceledException)
            {
                // ignore - a newer local change reset the delay
            }
        }, token);

        return Task.CompletedTask;
    }

    public async Task TriggerSyncAsync(
        Func<Task> syncAction,
        Func<Task> refreshAction,
        Action<string>? setMessage = null,
        Func<Task>? refreshUi = null,
        bool isAutomaticTrigger = true)
    {
        if (!IsSupported)
        {
            return;
        }

        if (!await _syncOperationGate.WaitAsync(0))
        {
            _syncRequested = true;
            return;
        }

        try
        {
            do
            {
                _syncRequested = false;
                var runningMessage = isAutomaticTrigger
                    ? "Changes detected. Running background sync..."
                    : "Starting sync...";

                setMessage?.Invoke(runningMessage);
                if (refreshUi is not null)
                {
                    await refreshUi();
                }

                await syncAction();
                await refreshAction();

                var completedMessage = isAutomaticTrigger ? "Background sync complete." : "Sync complete";
                setMessage?.Invoke(completedMessage);
                if (refreshUi is not null)
                {
                    await refreshUi();
                }

                isAutomaticTrigger = true;
            }
            while (_syncRequested);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PageSyncCoordinator:TriggerSyncAsync-Exception");
            setMessage?.Invoke($"Sync failed: {ex.Message}");
            if (refreshUi is not null)
            {
                await refreshUi();
            }
        }
        finally
        {
            if (refreshUi is not null)
            {
                await refreshUi();
            }
            _syncOperationGate.Release();
        }
    }

    public void Dispose()
    {
        _autoSyncDelayCts?.Cancel();
        _autoSyncDelayCts?.Dispose();
        _syncOperationGate.Dispose();
    }
}
