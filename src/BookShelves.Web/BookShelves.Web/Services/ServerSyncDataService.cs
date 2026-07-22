using BookShelves.Shared.Data.Interfaces;

namespace BookShelves.Web.Services;

internal class ServerSyncDataService : ISyncDataService
{
    public bool SupportsSync => false;

    public Task ServerSyncAsync()
    {
        throw new NotImplementedException();
    }
}
