using BookShelves.Shared.Data.Interfaces;

namespace BookShelves.Web.Client.Services.Client;

internal class SyncDataService : ISyncDataService
{
    public bool SupportsSync => false;

    public Task ServerSyncAsync()
    {
        throw new NotImplementedException();
    }
}
