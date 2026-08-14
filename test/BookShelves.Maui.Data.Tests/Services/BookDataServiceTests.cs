using BookShelves.Maui.Data.Infrastructure;
using BookShelves.Maui.Data.Models;
using BookShelves.Maui.Data.Services.Maui;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Moq;

namespace BookShelves.Maui.Data.Tests.Services;

public sealed class BookDataServiceTests
{
    [Fact]
    public async Task GetBooksAsync_MapsBooksAndAuthorDetails()
    {
        var booksRepo = new Mock<IRepository<Book>>();
        var authorsRepo = new Mock<IRepository<Author>>();
        var unitOfWork = new Mock<IUnitOfWork<SyncDbContext>>();
        var provider = CreateProvider(unitOfWork.Object);

        booksRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new[]
        {
            new Book { Id = "b1", Title = "Book 1", AuthorId = "a1", Description = "Desc" }
        });

        authorsRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new[]
        {
            new Author { Id = "a1", Name = "Author One" }
        });

        unitOfWork.Setup(x => x.GetRepository<Book>()).Returns(booksRepo.Object);
        unitOfWork.Setup(x => x.GetRepository<Author>()).Returns(authorsRepo.Object);

        var sut = new BookDataService(provider.Object);

        var result = (await sut.GetBooksAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("Book 1", result[0].Title);

        var mappedAuthor = result[0].Author;
        Assert.NotNull(mappedAuthor);
        Assert.Equal("a1", mappedAuthor!.Id);
        Assert.Equal("Author One", mappedAuthor.Name);
    }

    [Fact]
    public async Task CreateBookAsync_ReturnsTrue_WhenSaveChangesIsPositive()
    {
        var input = new BookViewModel
        {
            Title = "New Book",
            Description = "Description"
        };

        var repo = new Mock<IRepository<Book>>();
        var unitOfWork = new Mock<IUnitOfWork<SyncDbContext>>();
        var provider = CreateProvider(unitOfWork.Object);

        unitOfWork.Setup(x => x.GetRepository<Book>()).Returns(repo.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var sut = new BookDataService(provider.Object);

        var result = await sut.CreateBookAsync(input);

        Assert.True(result);
        repo.Verify(x => x.AddAsync(It.Is<Book>(b => b.Title == "New Book" && b.Description == "Description")), Times.Once);
    }

    [Fact]
    public async Task UpdateBookAsync_ReturnsFalse_WhenSaveChangesIsZero()
    {
        var input = new BookViewModel
        {
            Id = "book-1",
            Title = "Updated"
        };

        var repo = new Mock<IRepository<Book>>();
        var unitOfWork = new Mock<IUnitOfWork<SyncDbContext>>();
        var provider = CreateProvider(unitOfWork.Object);

        unitOfWork.Setup(x => x.GetRepository<Book>()).Returns(repo.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

        var sut = new BookDataService(provider.Object);

        var result = await sut.UpdateBookAsync(input);

        Assert.False(result);
        repo.Verify(x => x.UpdateAsync(It.Is<Book>(b => b.Id == "book-1" && b.Title == "Updated")), Times.Once);
    }

    [Fact]
    public async Task DeleteBookAsync_ReturnsTrue_WhenSaveChangesIsPositive()
    {
        var input = new BookViewModel
        {
            Id = "book-delete",
            Title = "Delete"
        };

        var repo = new Mock<IRepository<Book>>();
        var unitOfWork = new Mock<IUnitOfWork<SyncDbContext>>();
        var provider = CreateProvider(unitOfWork.Object);

        unitOfWork.Setup(x => x.GetRepository<Book>()).Returns(repo.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var sut = new BookDataService(provider.Object);

        var result = await sut.DeleteBookAsync(input);

        Assert.True(result);
        repo.Verify(x => x.DeleteAsync(It.Is<Book>(b => b.Id == "book-delete" && b.Title == "Delete")), Times.Once);
    }

    [Fact]
    public async Task GetBooksAsync_LeavesAuthorNull_WhenAuthorIsMissing()
    {
        var booksRepo = new Mock<IRepository<Book>>();
        var authorsRepo = new Mock<IRepository<Author>>();
        var unitOfWork = new Mock<IUnitOfWork<SyncDbContext>>();
        var provider = CreateProvider(unitOfWork.Object);

        booksRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(new[]
        {
            new Book { Id = "b2", Title = "Orphaned Book", AuthorId = "missing" }
        });
        authorsRepo.Setup(x => x.GetAllAsync()).ReturnsAsync(Array.Empty<Author>());

        unitOfWork.Setup(x => x.GetRepository<Book>()).Returns(booksRepo.Object);
        unitOfWork.Setup(x => x.GetRepository<Author>()).Returns(authorsRepo.Object);

        var sut = new BookDataService(provider.Object);

        var result = (await sut.GetBooksAsync()).Single();

        Assert.Null(result.Author);
    }

    private static Mock<IServiceProvider> CreateProvider(IUnitOfWork<SyncDbContext> unitOfWork)
    {
        var provider = new Mock<IServiceProvider>();
        provider.Setup(x => x.GetService(typeof(IUnitOfWork<SyncDbContext>))).Returns(unitOfWork);
        return provider;
    }
}
