using BookShelves.Maui.Data.Infrastructure;
using BookShelves.Shared.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BookShelves.Maui.Data.Services.Maui;

public class SyncDataService(IServiceProvider serviceProvider) : ISyncDataService
{
    public bool SupportsSync => true;

    public async Task ServerSyncAsync()
    {
        await using var uow = serviceProvider.GetRequiredService<ISyncUnitOfWork<SyncDbContext>>();
        await uow.SynchronizeAsync();
    }
}
