using BookShelves.Maui.Data.Infrastructure;
using BookShelves.Maui.Data.Services.Maui;
using BookShelves.Shared.Data.Interfaces;
using Moq;

namespace BookShelves.Maui.Data.Tests.Services;

public sealed class SyncDataServiceTests
{
    [Fact]
    public async Task ServerSyncAsync_ResolvesAndInvokesSyncUnitOfWork()
    {
        var syncUow = new Mock<ISyncUnitOfWork<SyncDbContext>>();
        var provider = new Mock<IServiceProvider>();

        provider
            .Setup(p => p.GetService(typeof(ISyncUnitOfWork<SyncDbContext>)))
            .Returns(syncUow.Object);

        var sut = new SyncDataService(provider.Object);

        await sut.ServerSyncAsync();

        syncUow.Verify(x => x.SynchronizeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
