using BookShelves.Maui.Data.SyncTest;
using BookShelves.Shared.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BookShelves.Maui.Data.Services;

public class MauiSyncDataService(IServiceProvider serviceProvider) : ISyncDataService
{
    public bool SupportsSync => true;

    public async Task ServerSyncAsync()
    {
        await using var uow = serviceProvider.GetRequiredService<ISyncUnitOfWork<SyncDbContext>>();
        await uow.SynchronizeAsync();
    }
}
