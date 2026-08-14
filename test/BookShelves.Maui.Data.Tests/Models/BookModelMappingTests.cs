using BookShelves.Maui.Data.Models;
using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Maui.Data.Tests.Models;

public sealed class BookModelMappingTests
{
    [Fact]
    public void FromBookViewModel_AssignsNewId_WhenMissingAndSetNewIdTrue()
    {
        var viewModel = new BookViewModel
        {
            Id = string.Empty,
            Title = "Title"
        };

        var result = Book.FromBookViewModel(viewModel, setNewId: true);

        Assert.False(string.IsNullOrWhiteSpace(result.Id));
    }

    [Fact]
    public void ToBookViewModel_MapsMatchingAuthor()
    {
        var updatedAt = DateTimeOffset.UtcNow;
        var model = new Book
        {
            Id = "book-1",
            Title = "The Book",
            AuthorId = "author-1",
            UpdatedAt = updatedAt,
            Version = "ver"
        };

        var authors = new[]
        {
            new Author { Id = "author-1", Name = "Author One" }
        };

        var result = model.ToBookViewModel(authors);

        Assert.Equal("book-1", result.Id);
        Assert.NotNull(result.Author);
        Assert.Equal("author-1", result.Author!.Id);
        Assert.Equal(updatedAt, result.LastUpdateTime);
        Assert.Equal("ver", result.Version);
    }
}
