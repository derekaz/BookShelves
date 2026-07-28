
using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Maui.Data.Models;

public class Book : OfflineClientEntity
{
    public string Title { get; set; } = string.Empty;

    public string? AuthorId { get; set; }

    public string? Description { get; set; }

    public DateTime? PublisherDate { get; set; }

    public BookViewModel ToBookViewModel(IEnumerable<Author> authors)
    {
        var authorMap = authors.ToDictionary(a => a.Id, a => new AuthorViewModel
        {
            Id = a.Id,
            Name = a.Name
        });

        return new BookViewModel()
        {
            Id = Id,
            Title = Title,
            Author = AuthorId != null && authorMap.TryGetValue(AuthorId, out var authorVm) ? authorVm : null,
            Description = Description,
            LastUpdateTime = UpdatedAt,
        };
    }

    public static Book FromBookViewModel(BookViewModel book, bool setNewId)
    {
        return new Book()
        {
            Id = string.IsNullOrEmpty(book.Id) ? (setNewId ? Guid.CreateVersion7().ToString() : string.Empty) : book.Id,
            Title = book.Title ?? string.Empty,
            AuthorId = book.Author?.Id,
            Description = book.Description,
            UpdatedAt = book.LastUpdateTime,
        };
    }
}
