using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Web.Shared.Data;

public class Author : DatasyncDto
{
    public const string AUTHORS_UNIQUEID_RECORD_ID = "**UNIQUEID**";

    //public string? Id { get; set; } = string.Empty;

    //public string IdValue => Id ?? string.Empty;

    public string? Name { get; set; } = string.Empty;

    public string? Bio { get; set; } = string.Empty;

    //public DateTimeOffset? UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public AuthorItemViewModel ToAuthorItemViewModel()
    {
        return new AuthorItemViewModel()
        {
            Id = Id,
            Name = Name,
            Biography = Bio,
            LastUpdateTime = UpdatedAt
        };
    }

    public static Author FromAuthorItemViewModel(AuthorItemViewModel author)
    {
        return new Author()
        {
            Id = author.Id ?? string.Empty,
            Name = author.Name,
            Bio = author.Biography,
            // UpdatedAt = author.LastUpdateTime ?? DateTime.UtcNow
        };
    }
}
