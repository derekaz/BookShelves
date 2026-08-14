using BookShelves.Shared.Presentation.ViewModels;
using BookShelves.Web.Shared.Data;

namespace BookShelves.Web.Shared.Tests.Data;

public sealed class AuthorTests
{
    [Fact]
    public void ToAuthorItemViewModel_MapsAllFields()
    {
        var updatedAt = DateTimeOffset.UtcNow;
        var author = new Author
        {
            Id = "author-1",
            Name = "Test Author",
            Bio = "Test bio",
            UpdatedAt = updatedAt,
            Version = "v1"
        };

        var result = author.ToAuthorItemViewModel();

        Assert.Equal("author-1", result.Id);
        Assert.Equal("Test Author", result.Name);
        Assert.Equal("Test bio", result.Biography);
        Assert.Equal(updatedAt, result.LastUpdateTime);
        Assert.Equal("v1", result.Version);
    }

    [Fact]
    public void FromAuthorItemViewModel_UsesUtcNow_WhenLastUpdateMissing()
    {
        var before = DateTimeOffset.UtcNow;
        var viewModel = new AuthorViewModel
        {
            Id = "author-2",
            Name = "Author",
            Biography = "Bio"
        };

        var result = Author.FromAuthorItemViewModel(viewModel);
        var after = DateTimeOffset.UtcNow;

        Assert.Equal("author-2", result.Id);
        Assert.NotNull(result.UpdatedAt);
        Assert.InRange(result.UpdatedAt!.Value, before, after);
    }
}
