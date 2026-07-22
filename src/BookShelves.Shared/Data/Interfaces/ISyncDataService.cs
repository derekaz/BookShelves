namespace BookShelves.Shared.Data.Interfaces;

public interface ISyncDataService
{
    Task ServerSyncAsync();
}
