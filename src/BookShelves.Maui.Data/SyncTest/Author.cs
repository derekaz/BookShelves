
using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Maui.Data.SyncTest;

public class Author : OfflineClientEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Bio { get; set; }


    public AuthorViewModel ToAuthorViewModel()
    {
        return new AuthorViewModel()
        {
            Id = Id,
            Name = Name,
            Biography = Bio,
            LastUpdateTime = UpdatedAt,
        };
    }

    public static Author FromAuthorViewModel(AuthorViewModel author, bool setNewId)
    {
        return new Author()
        {
            Id = string.IsNullOrEmpty(author.Id) ? (setNewId ? Guid.CreateVersion7().ToString() : string.Empty) : author.Id,
            Name = author.Name ?? string.Empty,
            Bio = author.Biography,
            UpdatedAt = author.LastUpdateTime,
        };
    }
}
