using CommunityToolkit.Datasync.Server.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BookShelves.WebApi.AuthorsDataAccess;

public class AuthorItem : EntityTableData
{
    [Required, MinLength(1)]
    public string Title { get; set; } = string.Empty;

    public string? ListId { get; set; }
}
