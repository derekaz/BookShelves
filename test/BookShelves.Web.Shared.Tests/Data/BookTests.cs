using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Web.Shared.Data;

namespace BookShelves.Web.Shared.Tests.Data;

public sealed class BookTests
{
    [Fact]
    public void ToBookViewModel_MapsAuthor_WhenMatchingAuthorExists()
    {
        var updatedAt = DateTimeOffset.UtcNow;
        var book = new Book
        {
            Id = "book-1",
            Title = "The Title",
            AuthorId = "author-1",
            Description = "Desc",
            PublishDate = DateTime.UtcNow.Date,
            UpdatedAt = updatedAt,
            Version = "etag"
        };

        var authors = new[]
        {
            new Author { Id = "author-1", Name = "Ada" }
        };

        var result = book.ToBookViewModel(authors);

        Assert.Equal("book-1", result.Id);
        Assert.Equal("The Title", result.Title);
        Assert.NotNull(result.Author);
        Assert.Equal("author-1", result.Author!.Id);
        Assert.Equal("Ada", result.Author.Name);
        Assert.Equal("Desc", result.Description);
        Assert.Equal(updatedAt, result.LastUpdateTime);
        Assert.Equal("etag", result.Version);
    }

    [Fact]
    public void ToBookViewModel_LeavesAuthorNull_WhenNoMatchingAuthorExists()
    {
        var book = new Book
        {
            Id = "book-2",
            Title = "No Author",
            AuthorId = "missing-author"
        };

        var result = book.ToBookViewModel(Array.Empty<Author>());

        Assert.Null(result.Author);
    }

    [Fact]
    public void FromBookViewModel_MapsFields()
    {
        var publishDate = DateTime.UtcNow.Date;
        var updatedAt = DateTimeOffset.UtcNow;
        var viewModel = new BookViewModel
        {
            Id = "book-3",
            Title = "Mapped",
            Author = new AuthorViewModel { Id = "author-9", Name = "Nina" },
            Description = "Description",
            PublishDate = publishDate,
            LastUpdateTime = updatedAt,
            Version = "v2"
        };

        var result = Book.FromBookViewModel(viewModel);

        Assert.Equal("book-3", result.Id);
        Assert.Equal("Mapped", result.Title);
        Assert.Equal("author-9", result.AuthorId);
        Assert.Equal("Description", result.Description);
        Assert.Equal(publishDate, result.PublishDate);
        Assert.Equal(updatedAt, result.UpdatedAt);
        Assert.Equal("v2", result.Version);
    }
}
