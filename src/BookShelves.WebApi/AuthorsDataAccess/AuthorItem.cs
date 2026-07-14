using CommunityToolkit.Datasync.Server.CosmosDb;
using System.ComponentModel.DataAnnotations;

namespace BookShelves.WebApi.AuthorsDataAccess;

public class AuthorItem : CosmosTableData<AuthorItem>
{
    [Required, MinLength(1)]    
    public string Name { get; set; } = string.Empty;

    public string? Bio { get; set; }
}
