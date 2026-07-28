using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Web.Shared.Data;

public class Author : DatasyncDto
{
    public string? Name { get; set; } = string.Empty;

    public string? Bio { get; set; } = string.Empty;

    public AuthorViewModel ToAuthorItemViewModel()
    {
        return new AuthorViewModel()
        {
            Id = Id,
            Name = Name,
            Biography = Bio,
            LastUpdateTime = UpdatedAt
        };
    }

    public static Author FromAuthorItemViewModel(AuthorViewModel author)
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
