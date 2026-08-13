using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Services;
using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookShelves.Shared.Tests.Services;

public sealed class PageSyncCoordinatorTests : IDisposable
{
    private readonly Mock<ISyncDataService> _syncDataService = new();
    private readonly Mock<ISyncProgressService> _syncProgressService = new();
    private readonly Mock<ILogger<PageSyncCoordinator>> _logger = new();
    private readonly PageSyncCoordinator _sut;

    public PageSyncCoordinatorTests()
    {
        _sut = new PageSyncCoordinator(
            _syncDataService.Object,
            _syncProgressService.Object,
            _logger.Object);
    }

    public void Dispose() => _sut.Dispose();

    // ── IsSupported ───────────────────────────────────────────────────────────

    [Fact]
    public void IsSupported_ReturnsTrue_WhenSyncDataServiceSupportsSync()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(true);

        Assert.True(_sut.IsSupported);
    }

    [Fact]
    public void IsSupported_ReturnsFalse_WhenSyncDataServiceDoesNotSupportSync()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(false);

        Assert.False(_sut.IsSupported);
    }

    // ── TriggerSyncAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task TriggerSyncAsync_DoesNotCallSyncAction_WhenSyncIsNotSupported()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(false);
        var syncCalled = false;

        await _sut.TriggerSyncAsync(
            syncAction: () => { syncCalled = true; return Task.CompletedTask; },
            refreshAction: () => Task.CompletedTask);

        Assert.False(syncCalled);
    }

    [Fact]
    public async Task TriggerSyncAsync_CallsSyncAction_WhenSyncIsSupported()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(true);
        var syncCalled = false;

        await _sut.TriggerSyncAsync(
            syncAction: () => { syncCalled = true; return Task.CompletedTask; },
            refreshAction: () => Task.CompletedTask);

        Assert.True(syncCalled);
    }

    [Fact]
    public async Task TriggerSyncAsync_CallsRefreshAction_WhenSyncIsSupported()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(true);
        var refreshCalled = false;

        await _sut.TriggerSyncAsync(
            syncAction: () => Task.CompletedTask,
            refreshAction: () => { refreshCalled = true; return Task.CompletedTask; });

        Assert.True(refreshCalled);
    }

    [Fact]
    public async Task TriggerSyncAsync_InvokesSetMessage_WithCompletedMessage()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(true);
        var messages = new List<string>();

        await _sut.TriggerSyncAsync(
            syncAction: () => Task.CompletedTask,
            refreshAction: () => Task.CompletedTask,
            setMessage: m => messages.Add(m));

        Assert.Contains(messages, m => m.Contains("complete", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task TriggerSyncAsync_InvokesSetMessage_WithSyncFailedMessage_WhenSyncActionThrows()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(true);
        var messages = new List<string>();

        await _sut.TriggerSyncAsync(
            syncAction: () => throw new InvalidOperationException("boom"),
            refreshAction: () => Task.CompletedTask,
            setMessage: m => messages.Add(m));

        Assert.Contains(messages, m => m.StartsWith("Sync failed:", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TriggerSyncAsync_DoesNotThrow_WhenSyncActionThrows()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(true);

        var exception = await Record.ExceptionAsync(() =>
            _sut.TriggerSyncAsync(
                syncAction: () => throw new InvalidOperationException("boom"),
                refreshAction: () => Task.CompletedTask));

        Assert.Null(exception);
    }

    [Fact]
    public async Task TriggerSyncAsync_CallsRefreshUi_WhenProvided()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(true);
        var refreshUiCallCount = 0;

        await _sut.TriggerSyncAsync(
            syncAction: () => Task.CompletedTask,
            refreshAction: () => Task.CompletedTask,
            refreshUi: () => { refreshUiCallCount++; return Task.CompletedTask; });

        Assert.True(refreshUiCallCount > 0);
    }

    // ── QueueAutomaticSyncAsync ───────────────────────────────────────────────

    [Fact]
    public async Task QueueAutomaticSyncAsync_ReturnsImmediately_WhenSyncIsNotSupported()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(false);
        var syncCalled = false;

        await _sut.QueueAutomaticSyncAsync(
            syncAction: () => { syncCalled = true; return Task.CompletedTask; },
            refreshAction: () => Task.CompletedTask);

        Assert.False(syncCalled);
    }

    [Fact]
    public async Task QueueAutomaticSyncAsync_ReportsPendingStage_WhenSyncIsSupported()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(true);

        await _sut.QueueAutomaticSyncAsync(
            syncAction: () => Task.CompletedTask,
            refreshAction: () => Task.CompletedTask);

        _syncProgressService.Verify(
            s => s.ReportStage(SyncStage.Pending, It.IsAny<string>(), null, null),
            Times.Once);
    }

    [Fact]
    public async Task QueueAutomaticSyncAsync_InvokesSetMessage_WithPendingMessage()
    {
        _syncDataService.Setup(s => s.SupportsSync).Returns(true);
        string? message = null;

        await _sut.QueueAutomaticSyncAsync(
            syncAction: () => Task.CompletedTask,
            refreshAction: () => Task.CompletedTask,
            setMessage: m => message = m);

        Assert.NotNull(message);
        Assert.Contains("Waiting to sync", message, StringComparison.OrdinalIgnoreCase);
    }
}
