namespace BookShelves.Shared.Data.Interfaces;

public interface IBooksSyncService
{
    Task BeginSyncAsync();
}
