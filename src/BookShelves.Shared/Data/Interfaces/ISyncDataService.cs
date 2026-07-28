namespace BookShelves.Shared.Data.Interfaces;

public interface ISyncDataService
{
    bool SupportsSync { get; }

    Task ServerSyncAsync();
}
