using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace BookShelves.WasmApi.DataAccess;

public class UniqueIdRepository : CosmosRepository<UniqueId>
{
    public UniqueIdRepository(
        ILogger<UniqueIdRepository> logger,
        CosmosClient client,
        string databaseName,
        string containerName
        ) : base(logger, client, databaseName, containerName) 
    {
        logger.LogInformation("UniqueIdRepository-Constructor");
    }
}
