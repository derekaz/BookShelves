using BookShelves.Shared.Presentation.ViewModels;
using AuthorsIndexPage = BookShelves.Shared.Components.Pages.Authors.Index;
using BooksIndexPage = BookShelves.Shared.Components.Pages.Books.Index;

namespace BookShelves.Shared.Tests.Components.Pages;

public sealed class AuthorBookIndexLogicTests
{
    [Fact]
    public void AuthorsIndex_ItemMatchesSearch_MatchesNameOrBiography()
    {
        var sut = new AuthorsIndexPage();
        var item = new AuthorViewModel
        {
            Name = "Octavia Butler",
            Biography = "Science fiction author"
        };

        var nameMatch = InvokeItemMatchesSearch(sut, item, "octavia");
        var bioMatch = InvokeItemMatchesSearch(sut, item, "science");
        var miss = InvokeItemMatchesSearch(sut, item, "history");

        Assert.True(nameMatch);
        Assert.True(bioMatch);
        Assert.False(miss);
    }

    [Fact]
    public void AuthorsIndex_CloneAuthor_CreatesIndependentCopy()
    {
        var sut = new AuthorsIndexPage();
        var source = new AuthorViewModel
        {
            Id = "author-1",
            Name = "Author",
            Biography = "Bio",
            LastUpdateTime = DateTimeOffset.UtcNow
        };

        var clone = InvokeCloneAuthor(sut, source);

        Assert.NotSame(source, clone);
        Assert.Equal(source.Id, clone.Id);
        Assert.Equal(source.Name, clone.Name);
        Assert.Equal(source.Biography, clone.Biography);
        Assert.Equal(source.LastUpdateTime, clone.LastUpdateTime);
    }

    [Fact]
    public void BooksIndex_ItemMatchesSearch_MatchesTitleAuthorOrDescription()
    {
        var sut = new BooksIndexPage();
        var item = new BookViewModel
        {
            Title = "The Hobbit",
            Description = "A fantasy journey",
            Author = new AuthorViewModel { Name = "Tolkien" }
        };

        var titleMatch = InvokeItemMatchesSearch(sut, item, "hobbit");
        var authorMatch = InvokeItemMatchesSearch(sut, item, "tolkien");
        var descriptionMatch = InvokeItemMatchesSearch(sut, item, "journey");
        var miss = InvokeItemMatchesSearch(sut, item, "mystery");

        Assert.True(titleMatch);
        Assert.True(authorMatch);
        Assert.True(descriptionMatch);
        Assert.False(miss);
    }

    [Fact]
    public void BooksIndex_CloneBook_ClonesAuthorSnapshot()
    {
        var sut = new BooksIndexPage();
        var source = new BookViewModel
        {
            Id = "book-1",
            Title = "Title",
            Description = "Desc",
            PublishDate = new DateTime(2024, 1, 2),
            LastUpdateTime = DateTimeOffset.UtcNow,
            Author = new AuthorViewModel
            {
                Id = "author-9",
                Name = "Author",
                Biography = "Bio",
                LastUpdateTime = DateTimeOffset.UtcNow
            }
        };

        var clone = InvokeCloneBook(sut, source);

        Assert.NotSame(source, clone);
        Assert.Equal(source.Id, clone.Id);
        Assert.Equal(source.Title, clone.Title);
        Assert.Equal(source.Description, clone.Description);
        Assert.Equal(source.PublishDate, clone.PublishDate);
        Assert.Equal(source.LastUpdateTime, clone.LastUpdateTime);
        Assert.NotNull(clone.Author);
        Assert.NotSame(source.Author, clone.Author);
        Assert.Equal(source.Author!.Id, clone.Author!.Id);
        Assert.Equal(source.Author.Name, clone.Author.Name);
    }

    private static bool InvokeItemMatchesSearch(object instance, object model, string query)
    {
        var method = instance.GetType().GetMethod("ItemMatchesSearch", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);
        return (bool)method!.Invoke(instance, [model, query])!;
    }

    private static AuthorViewModel InvokeCloneAuthor(AuthorsIndexPage instance, AuthorViewModel source)
    {
        var method = instance.GetType().GetMethod("CloneAuthor", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);
        return (AuthorViewModel)method!.Invoke(instance, [source])!;
    }

    private static BookViewModel InvokeCloneBook(BooksIndexPage instance, BookViewModel source)
    {
        var method = instance.GetType().GetMethod("CloneBook", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);
        return (BookViewModel)method!.Invoke(instance, [source])!;
    }
}
