using BookShelves.Shared.Data.Interfaces;

namespace BookShelves.Web.Services.Server;

internal class SyncDataService : ISyncDataService
{
    public bool SupportsSync => false;

    public Task ServerSyncAsync()
    {
        throw new NotImplementedException();
    }
}
