using BookShelves.Shared.Data.Interfaces;

namespace BookShelves.WasmSwa.Services
{
    internal class WasmSyncDataService : ISyncDataService
    {
        public bool SupportsSync => false;

        public Task ServerSyncAsync()
        {
            throw new NotImplementedException();
        }
    }
}
