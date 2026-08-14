using BookShelves.Shared.Components.Bases;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services;
using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.Extensions.Logging;
using Moq;
using MudBlazor;

namespace BookShelves.Shared.Tests.Components;

public sealed class DataViewPageBaseTests
{
    [Fact]
    public async Task OnInitializedAsync_LoadsDataServiceOnce()
    {
        var sut = CreateSut();

        await sut.InvokeOnInitializedAsync();

        Assert.Equal(1, sut.LoadDataServiceAsyncCallCount);
    }

    [Fact]
    public async Task OnSyncProgressChanged_UpdatesSyncStatusAndFormatsMessage_WhenCountsProvided()
    {
        var syncProgress = new TestSyncProgressService();
        var sut = CreateSut(syncProgressService: syncProgress);
        await sut.InvokeOnInitializedAsync();

        syncProgress.Report(new SyncProgressEventArgs
        {
            SyncStage = SyncStage.Pulling,
            Message = "Pulling remote changes",
            Current = 2,
            Total = 5,
            CurrentStep = 2,
            TotalSteps = 3,
            ProgressPercentage = 66
        });

        Assert.Equal(SyncStage.Pulling, sut.CurrentStage);
        Assert.Equal(66, sut.ProgressPercentage);
        Assert.Equal(2, sut.CurrentStep);
        Assert.Equal(3, sut.TotalSteps);
        Assert.Equal("Pulling remote changes (2/5)", sut.SyncStatusMessage);
    }

    [Fact]
    public async Task OnSyncProgressChanged_UsesPlainMessage_WhenCountsMissing()
    {
        var syncProgress = new TestSyncProgressService();
        var sut = CreateSut(syncProgressService: syncProgress);
        await sut.InvokeOnInitializedAsync();

        syncProgress.Report(new SyncProgressEventArgs
        {
            SyncStage = SyncStage.Started,
            Message = "Synchronization started",
            CurrentStep = 0,
            TotalSteps = 3,
            ProgressPercentage = 0
        });

        Assert.Equal(SyncStage.Started, sut.CurrentStage);
        Assert.Equal("Synchronization started", sut.SyncStatusMessage);
    }

    [Fact]
    public async Task OnSyncProgressChanged_WhenCompleted_UpdatesCompletionState()
    {
        var syncProgress = new TestSyncProgressService();
        var sut = CreateSut(syncProgressService: syncProgress);
        await sut.InvokeOnInitializedAsync();

        syncProgress.Report(new SyncProgressEventArgs
        {
            SyncStage = SyncStage.Completed,
            Message = "Synchronization complete",
            CurrentStep = 3,
            TotalSteps = 3,
            ProgressPercentage = 100
        });

        Assert.Equal(SyncStage.Completed, sut.CurrentStage);
        Assert.Equal(100, sut.ProgressPercentage);
        Assert.Equal("Synchronization complete", sut.SyncStatusMessage);
    }

    [Fact]
    public async Task Dispose_UnsubscribesFromSyncProgressEvents()
    {
        var syncProgress = new TestSyncProgressService();
        var sut = CreateSut(syncProgressService: syncProgress);
        await sut.InvokeOnInitializedAsync();

        syncProgress.Report(new SyncProgressEventArgs
        {
            SyncStage = SyncStage.Pending,
            Message = "Waiting to sync"
        });

        sut.Dispose();

        syncProgress.Report(new SyncProgressEventArgs
        {
            SyncStage = SyncStage.Completed,
            Message = "Synchronization complete"
        });

        Assert.Equal(SyncStage.Pending, sut.CurrentStage);
        Assert.Equal("Waiting to sync", sut.SyncStatusMessage);
    }

    [Fact]
    public void QueueAutomaticSync_DoesNotQueue_WhenSyncNotSupported()
    {
        var pageSyncCoordinator = new Mock<IPageSyncCoordinator>();
        pageSyncCoordinator.SetupGet(x => x.IsSupported).Returns(false);

        var sut = CreateSut(pageSyncCoordinator: pageSyncCoordinator);

        sut.InvokeQueueAutomaticSync();

        pageSyncCoordinator.Verify(
            x => x.QueueAutomaticSyncAsync(
                It.IsAny<Func<Task>>(),
                It.IsAny<Func<Task>>(),
                It.IsAny<Action<string>>(),
                It.IsAny<Func<Task>>(),
                It.IsAny<bool>()),
            Times.Never);
    }

    [Fact]
    public void QueueAutomaticSync_Queues_WhenSyncSupported()
    {
        var pageSyncCoordinator = new Mock<IPageSyncCoordinator>();
        pageSyncCoordinator.SetupGet(x => x.IsSupported).Returns(true);
        pageSyncCoordinator
            .Setup(x => x.QueueAutomaticSyncAsync(
                It.IsAny<Func<Task>>(),
                It.IsAny<Func<Task>>(),
                It.IsAny<Action<string>>(),
                It.IsAny<Func<Task>>(),
                It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(pageSyncCoordinator: pageSyncCoordinator);

        sut.InvokeQueueAutomaticSync();

        pageSyncCoordinator.Verify(
            x => x.QueueAutomaticSyncAsync(
                It.IsAny<Func<Task>>(),
                It.IsAny<Func<Task>>(),
                It.IsAny<Action<string>>(),
                It.IsAny<Func<Task>>(),
                true),
            Times.Once);
    }

    [Fact]
    public void FilterFunc_ReturnsTrue_WhenSearchStringIsEmpty()
    {
        var sut = CreateSut();

        var result = sut.InvokeFilter("any value");

        Assert.True(result);
    }

    [Fact]
    public void FilterFunc_UsesItemMatchesSearch_WhenSearchStringProvided()
    {
        var sut = CreateSut();
        sut.SetSearchString("needle");

        var matches = sut.InvokeFilter("contains needle text");
        var misses = sut.InvokeFilter("different");

        Assert.True(matches);
        Assert.False(misses);
    }

    [Fact]
    public async Task ProcessDialogResultAsync_SetsErrorMessage_WhenResultIsNull()
    {
        var sut = CreateSut();
        var dialog = new Mock<IDialogReference>();
        dialog.SetupGet(x => x.Result).Returns(Task.FromResult<DialogResult?>(null));

        await sut.InvokeProcessDialogResultAsync(dialog.Object, "Canceled");

        Assert.Equal("Error completing the request", sut.Message);
        Assert.Equal(0, sut.LoadDataServiceAsyncCallCount);
    }

    [Fact]
    public async Task ProcessDialogResultAsync_SetsCancelMessage_WhenCanceled()
    {
        var sut = CreateSut();
        var dialog = new Mock<IDialogReference>();
        dialog.SetupGet(x => x.Result).Returns(Task.FromResult<DialogResult?>(DialogResult.Cancel()));

        await sut.InvokeProcessDialogResultAsync(dialog.Object, "Canceled by user");

        Assert.Equal("Canceled by user", sut.Message);
        Assert.Equal(0, sut.LoadDataServiceAsyncCallCount);
    }

    [Fact]
    public async Task ProcessDialogResultAsync_RefreshesDataAndQueuesSync_WhenCompleted()
    {
        var pageSyncCoordinator = new Mock<IPageSyncCoordinator>();
        pageSyncCoordinator.SetupGet(x => x.IsSupported).Returns(true);
        pageSyncCoordinator
            .Setup(x => x.QueueAutomaticSyncAsync(
                It.IsAny<Func<Task>>(),
                It.IsAny<Func<Task>>(),
                It.IsAny<Action<string>>(),
                It.IsAny<Func<Task>>(),
                It.IsAny<bool>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(pageSyncCoordinator: pageSyncCoordinator);
        var dialog = new Mock<IDialogReference>();
        dialog.SetupGet(x => x.Result).Returns(Task.FromResult<DialogResult?>(DialogResult.Ok("Saved")));

        await sut.InvokeProcessDialogResultAsync(dialog.Object, "Canceled");

        Assert.Equal("Saved", sut.Message);
        Assert.Equal(1, sut.LoadDataServiceAsyncCallCount);
        pageSyncCoordinator.Verify(
            x => x.QueueAutomaticSyncAsync(
                It.IsAny<Func<Task>>(),
                It.IsAny<Func<Task>>(),
                It.IsAny<Action<string>>(),
                It.IsAny<Func<Task>>(),
                true),
            Times.Once);
    }

    private static TestDataViewPage CreateSut(
        Mock<IPageSyncCoordinator>? pageSyncCoordinator = null,
        Mock<ISyncDataService>? syncDataService = null,
        ISyncProgressService? syncProgressService = null)
    {
        var sut = new TestDataViewPage();
        sut.InitializeDependencies(
            dialogService: Mock.Of<IDialogService>(),
            snackbar: Mock.Of<ISnackbar>(),
            pageSyncCoordinator: (pageSyncCoordinator ?? new Mock<IPageSyncCoordinator>()).Object,
            syncDataService: (syncDataService ?? new Mock<ISyncDataService>()).Object,
            syncProgressService: syncProgressService ?? new Mock<ISyncProgressService>().Object,
            logger: Mock.Of<ILogger<DataViewPageBase<string>>>());

        return sut;
    }

    private sealed class TestDataViewPage : DataViewPageBase<string>
    {
        public string Message => message;

        public string SyncStatusMessage => syncStatus.Message ?? string.Empty;

        public SyncStage CurrentStage => syncStatus.CurrentStage;

        public int ProgressPercentage => syncStatus.ProgressPercentage;

        public int CurrentStep => syncStatus.CurrentStep;

        public int TotalSteps => syncStatus.TotalSteps;

        public int LoadDataServiceAsyncCallCount { get; private set; }

        public void InitializeDependencies(
            IDialogService dialogService,
            ISnackbar snackbar,
            IPageSyncCoordinator pageSyncCoordinator,
            ISyncDataService syncDataService,
            ISyncProgressService syncProgressService,
            ILogger<DataViewPageBase<string>> logger)
        {
            DialogService = dialogService;
            Snackbar = snackbar;
            PageSyncCoordinator = pageSyncCoordinator;
            SyncDataService = syncDataService;
            SyncProgressService = syncProgressService;
            Logger = logger;
        }

        public Task InvokeOnInitializedAsync() => OnInitializedAsync();

        public void SetSearchString(string value) => searchString = value;

        public bool InvokeFilter(string item) => FilterFunc(item);

        public Task InvokeProcessDialogResultAsync(IDialogReference dialog, string cancelLogMessage)
            => ProcessDialogResultAsync(dialog, cancelLogMessage);

        public void InvokeQueueAutomaticSync() => QueueAutomaticSync();

        protected override Task LoadDataServiceAsync()
        {
            LoadDataServiceAsyncCallCount++;
            return Task.CompletedTask;
        }

        protected override bool ItemMatchesSearch(string item, string query)
            => item.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TestSyncProgressService : ISyncProgressService
    {
        public event EventHandler<SyncProgressEventArgs>? ProgressChanged;

        public void Report(SyncProgressEventArgs args)
            => ProgressChanged?.Invoke(this, args);

        public void ReportStage(SyncStage stage, string message, int? current = null, int? total = null)
            => Report(new SyncProgressEventArgs
            {
                SyncStage = stage,
                Stage = stage.ToString(),
                Message = message,
                Current = current,
                Total = total,
                TotalSteps = 3
            });
    }
}
