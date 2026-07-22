using BookShelves.Shared.Data.Interfaces;

namespace BookShelves.Web.Client.Services;

internal class ClientSyncDataService : ISyncDataService
{
    public bool SupportsSync => false;

    public Task ServerSyncAsync()
    {
        throw new NotImplementedException();
    }
}
