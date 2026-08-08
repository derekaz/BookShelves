using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Shared.Services;
using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace BookShelves.Shared.Components.Bases;

public abstract class DataViewPageBase<TItem> : ComponentBase, IDisposable
{
    [Inject] protected IDialogService DialogService { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected ISyncProgressService SyncProgressService { get; set; } = default!;
    [Inject] protected IPageSyncCoordinator PageSyncCoordinator { get; set; } = default!;
    [Inject] protected ISyncDataService SyncDataService { get; set; } = default!;
    [Inject] protected ILogger<DataViewPageBase<TItem>> Logger { get; set; } = default!;

#if NET10_0
    [PersistentState]
#endif
    protected IEnumerable<TItem>? Items { get; set; }

    protected string message = string.Empty;

    protected string searchString = string.Empty;

    protected readonly SyncStatusViewModel syncStatus = new()
    {
        ShowWhenIdle = false,
        ShowProgress = true
    };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            SyncProgressService.ProgressChanged += OnSyncProgressChanged;
            await FetchDataAsync();
        }
        catch (Microsoft.Identity.Client.MsalUiRequiredException) { /* Identity interceptors */ }
        catch (NavigationException) { /* Router interceptors */ }
        catch (Exception ex)
        {
            Logger.LogError(ex, "{Page}:OnInitializedAsync-Exception", GetType().Name);
        }
    }

    protected abstract Task LoadDataServiceAsync();
    protected abstract bool ItemMatchesSearch(TItem item, string query);

    protected async Task FetchDataAsync()
    {
        await LoadDataServiceAsync();
    }

    protected bool FilterFunc(TItem item)
    {
        if (string.IsNullOrWhiteSpace(searchString)) return true;
        return ItemMatchesSearch(item, searchString);
    }

    protected async Task ProcessDialogResultAsync(IDialogReference dialog, string cancelLogMessage)
    {
        var result = await dialog.Result;
        if (result == null)
        {
            message = "Error completing the request";
        }
        else if (!result.Canceled)
        {
            message = result.Data?.ToString() ?? string.Empty;
            await FetchDataAsync();
            QueueAutomaticSync();
        }
        else
        {
            message = cancelLogMessage;
        }
    }

    private void OnSyncProgressChanged(object? sender, SyncProgressEventArgs e)
    {
        try
        {
            syncStatus.CurrentStage = e.SyncStage;
            syncStatus.ProgressPercentage = e.ProgressPercentage;
            syncStatus.CurrentStep = e.CurrentStep;
            syncStatus.TotalSteps = e.TotalSteps;
            syncStatus.Message = BuildSyncMessage(e);

            if (e.SyncStage == SyncStage.Completed)
            {
                _ = InvokeAsync(async () =>
                {
                    await FetchDataAsync();
                    StateHasChanged();
                });
            }

            _ = InvokeAsync(StateHasChanged);
        }
        catch { /* best-effort background safety block */ }
    }

    private string BuildSyncMessage(SyncProgressEventArgs? e)
    {
        if (e == null) return string.Empty;
        return e.Current.HasValue && e.Total.HasValue && e.Total.Value > 0
            ? $"{e.Message} ({e.Current}/{e.Total})"
            : e.Message ?? string.Empty;
    }

    protected void QueueAutomaticSync()
    {
        if (!PageSyncCoordinator.IsSupported) return;

        _ = PageSyncCoordinator.QueueAutomaticSyncAsync(
            syncAction: () => SyncDataService.ServerSyncAsync(),
            refreshAction: () => FetchDataAsync(),
            setMessage: value => message = value,
            refreshUi: () => InvokeAsync(StateHasChanged));
    }

    public void Dispose()
    {
        try { SyncProgressService.ProgressChanged -= OnSyncProgressChanged; } catch { }
        GC.SuppressFinalize(this);
    }
}
