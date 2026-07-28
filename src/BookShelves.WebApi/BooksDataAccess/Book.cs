using CommunityToolkit.Datasync.Server.CosmosDb;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace BookShelves.WebApi.BooksDataAccess;

public class Book : CosmosTableData<Book>
{
    [Required, MinLength(1)]
    public string Title { get; set; } = string.Empty;

    public string? AuthorId { get; set; }

    public string? Description { get; set; }

    public DateTime? PublishedDate { get; set; }
}
