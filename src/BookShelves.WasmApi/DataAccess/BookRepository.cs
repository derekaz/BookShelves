using BookShelves.WebShared.Data;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace BookShelves.WasmApi.DataAccess;

public class BookRepository : CosmosRepository<Book>
{
    public BookRepository(
        ILogger<BookRepository> logger,
        CosmosClient client,
        string databaseName,
        string containerName
        ) : base(logger, client, databaseName, containerName) { }
}
