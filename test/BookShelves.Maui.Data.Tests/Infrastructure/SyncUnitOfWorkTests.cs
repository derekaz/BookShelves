using BookShelves.Maui.Data.Infrastructure;
using BookShelves.Maui.Data.Interfaces;
using BookShelves.Shared.Services;
using BookShelves.Shared.Services.ServiceInterfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookShelves.Maui.Data.Tests.Infrastructure;

public sealed class SyncUnitOfWorkTests
{
    [Fact]
    public async Task SynchronizeAsync_WhenSyncSucceeds_ReportsStartedAndCompleted()
    {
        var progress = new CapturingSyncProgressService();
        await using var context = CreateContext();
        await using var sut = new SyncUnitOfWork<SyncDbContext>(new SingleContextFactory(context), progress, Mock.Of<ILogger<SyncUnitOfWork<SyncDbContext>>>());

        await sut.SynchronizeAsync();

        Assert.Equal(1, context.SynchronizeCallCount);
        Assert.Collection(
            progress.StageReports,
            item =>
            {
                Assert.Equal(SyncStage.Started, item.Stage);
                Assert.Equal("Synchronization started", item.Message);
            },
            item =>
            {
                Assert.Equal(SyncStage.Completed, item.Stage);
                Assert.Equal("Synchronization complete", item.Message);
            });
    }

    [Fact]
    public async Task SynchronizeAsync_WhenSyncThrows_ReportsFailedAndRethrows()
    {
        var progress = new CapturingSyncProgressService();
        await using var context = CreateContext(throwOnSynchronize: true);
        await using var sut = new SyncUnitOfWork<SyncDbContext>(new SingleContextFactory(context), progress, Mock.Of<ILogger<SyncUnitOfWork<SyncDbContext>>>());

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SynchronizeAsync());

        Assert.Equal("sync failed", ex.Message);
        Assert.Equal(1, context.SynchronizeCallCount);
        Assert.Collection(
            progress.StageReports,
            item =>
            {
                Assert.Equal(SyncStage.Started, item.Stage);
                Assert.Equal("Synchronization started", item.Message);
            },
            item =>
            {
                Assert.Equal(SyncStage.Failed, item.Stage);
                Assert.Equal("Synchronization failed: sync failed", item.Message);
            });
    }

    [Fact]
    public async Task SynchronizeAsync_ForwardsCancellationToken_ToSyncDbContext()
    {
        var progress = new CapturingSyncProgressService();
        await using var context = CreateContext();
        await using var sut = new SyncUnitOfWork<SyncDbContext>(new SingleContextFactory(context), progress, Mock.Of<ILogger<SyncUnitOfWork<SyncDbContext>>>());

        var cancellationToken = new CancellationTokenSource().Token;

        await sut.SynchronizeAsync(cancellationToken);

        Assert.Equal(cancellationToken, context.LastCancellationToken);
    }

    private static TestSyncDbContext CreateContext(bool throwOnSynchronize = false)
    {
        var options = new DbContextOptionsBuilder<SyncDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var syncApiClient = new Mock<ISyncApiClient>();
        syncApiClient
            .Setup(x => x.HttpClient)
            .Returns(new HttpClient { BaseAddress = new Uri("https://example.test/") });

        return new TestSyncDbContext(options, Mock.Of<ILogger<SyncDbContext>>(), syncApiClient.Object)
        {
            ThrowOnSynchronize = throwOnSynchronize
        };
    }

    private sealed class SingleContextFactory(SyncDbContext context) : IDbContextFactory<SyncDbContext>
    {
        public SyncDbContext CreateDbContext() => context;
    }

    private sealed class TestSyncDbContext(
        DbContextOptions<SyncDbContext> options,
        ILogger<SyncDbContext> logger,
        ISyncApiClient syncApiClient)
        : SyncDbContext(options, logger, syncApiClient)
    {
        public bool ThrowOnSynchronize { get; init; }
        public int SynchronizeCallCount { get; private set; }
        public CancellationToken LastCancellationToken { get; private set; }

        public override Task SynchronizeAsync(CancellationToken cancellationToken = default)
        {
            SynchronizeCallCount++;
            LastCancellationToken = cancellationToken;

            if (ThrowOnSynchronize)
            {
                throw new InvalidOperationException("sync failed");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class CapturingSyncProgressService : ISyncProgressService
    {
        public event EventHandler<SyncProgressEventArgs>? ProgressChanged;

        public List<(SyncStage Stage, string Message)> StageReports { get; } = [];

        public void Report(SyncProgressEventArgs args)
        {
            ProgressChanged?.Invoke(this, args);
        }

        public void ReportStage(SyncStage stage, string message, int? current = null, int? total = null)
        {
            StageReports.Add((stage, message));
            ProgressChanged?.Invoke(this, new SyncProgressEventArgs
            {
                SyncStage = stage,
                Message = message,
                Current = current,
                Total = total
            });
        }
    }
}
