using BookShelves.WebApi.DataAccess;
using Microsoft.Azure.Cosmos;

namespace BookShelves.WebApi.BooksDataAccess;

public class BookRepository : CosmosRepository<Book>
{
    public BookRepository(
        ILogger<BookRepository> logger,
        CosmosClient client,
        string databaseName,
        string containerName
        ) : base(logger, client, databaseName, containerName)
    {
        logger.LogInformation("BookRepository-Constructor");
    }
}
