using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Web.Shared.Data;

public class Book : DatasyncDto
{
    public string Title { get; set; } = string.Empty;

    public string? AuthorId { get; set; }

    public string? Description { get; set; }

    public DateTime? PublishDate { get; set; }


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
            PublishDate = PublishDate,
            LastUpdateTime = UpdatedAt,
            Version = Version
        };
    }

    public static Book FromBookViewModel(BookViewModel book)
    {
        return new Book()
        {
            Id = book.Id ?? string.Empty,
            Title = book.Title,
            AuthorId = book.Author?.Id,
            Description = book.Description,
            PublishDate = book.PublishDate,
            UpdatedAt = book.LastUpdateTime,
            Version = book.Version
        };
    }

}
