using BookShelves.Maui.Data.Infrastructure;
using BookShelves.Maui.Data.Models;
using BookShelves.Maui.Data.Services.Maui;
using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;
using Moq;

namespace BookShelves.Maui.Data.Tests.Services;

public sealed class AuthorDataServiceTests
{
    [Fact]
    public async Task CreateAuthorAsync_AddsMappedEntityAndReturnsTrue_WhenSaveChangesIsPositive()
    {
        var input = new AuthorViewModel
        {
            Name = "Octavia Butler",
            Biography = "Author"
        };

        var repo = new Mock<IRepository<Author>>();
        var unitOfWork = new Mock<IUnitOfWork<SyncDbContext>>();
        var provider = CreateProvider(unitOfWork.Object);
        Author? captured = null;

        repo.Setup(x => x.AddAsync(It.IsAny<Author>()))
            .Callback<Author>(entity => captured = entity)
            .Returns(Task.CompletedTask);
        unitOfWork.Setup(x => x.GetRepository<Author>()).Returns(repo.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var sut = new AuthorDataService(provider.Object);

        var result = await sut.CreateAuthorAsync(input);

        Assert.True(result);
        Assert.NotNull(captured);
        Assert.Equal("Octavia Butler", captured!.Name);
        Assert.Equal("Author", captured.Bio);
        Assert.False(string.IsNullOrWhiteSpace(captured.Id));
        Assert.NotNull(captured.UpdatedAt);
    }

    [Fact]
    public async Task GetAuthorsAsync_MapsEntitiesToViewModels()
    {
        var repo = new Mock<IRepository<Author>>();
        var unitOfWork = new Mock<IUnitOfWork<SyncDbContext>>();
        var provider = CreateProvider(unitOfWork.Object);

        repo.Setup(x => x.GetAllAsync()).ReturnsAsync(new[]
        {
            new Author { Id = "a1", Name = "Author 1", Bio = "Bio 1" },
            new Author { Id = "a2", Name = "Author 2", Bio = "Bio 2" }
        });

        unitOfWork.Setup(x => x.GetRepository<Author>()).Returns(repo.Object);

        var sut = new AuthorDataService(provider.Object);

        var result = (await sut.GetAuthorsAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("a1", result[0].Id);
        Assert.Equal("Author 1", result[0].Name);
        Assert.Equal("Bio 1", result[0].Biography);
    }

    [Fact]
    public async Task UpdateAuthorAsync_ReturnsFalse_WhenSaveChangesIsZero()
    {
        var input = new AuthorViewModel
        {
            Id = "author-1",
            Name = "Updated",
            Biography = "Updated bio"
        };

        var repo = new Mock<IRepository<Author>>();
        var unitOfWork = new Mock<IUnitOfWork<SyncDbContext>>();
        var provider = CreateProvider(unitOfWork.Object);

        unitOfWork.Setup(x => x.GetRepository<Author>()).Returns(repo.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

        var sut = new AuthorDataService(provider.Object);

        var result = await sut.UpdateAuthorAsync(input);

        Assert.False(result);
        repo.Verify(x => x.UpdateAsync(It.Is<Author>(a => a.Id == "author-1" && a.Name == "Updated")), Times.Once);
    }

    [Fact]
    public async Task DeleteAuthorAsync_DeletesMappedEntityAndReturnsTrue_WhenSaveChangesIsPositive()
    {
        var input = new AuthorViewModel
        {
            Id = "author-delete",
            Name = "Delete"
        };

        var repo = new Mock<IRepository<Author>>();
        var unitOfWork = new Mock<IUnitOfWork<SyncDbContext>>();
        var provider = CreateProvider(unitOfWork.Object);

        unitOfWork.Setup(x => x.GetRepository<Author>()).Returns(repo.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        var sut = new AuthorDataService(provider.Object);

        var result = await sut.DeleteAuthorAsync(input);

        Assert.True(result);
        repo.Verify(x => x.DeleteAsync(It.Is<Author>(a => a.Id == "author-delete" && a.Name == "Delete")), Times.Once);
    }

    [Fact]
    public async Task CreateAuthorAsync_ReturnsFalse_WhenSaveChangesIsZero()
    {
        var input = new AuthorViewModel
        {
            Name = "No Save",
            Biography = "No persist"
        };

        var repo = new Mock<IRepository<Author>>();
        var unitOfWork = new Mock<IUnitOfWork<SyncDbContext>>();
        var provider = CreateProvider(unitOfWork.Object);

        unitOfWork.Setup(x => x.GetRepository<Author>()).Returns(repo.Object);
        unitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(0);

        var sut = new AuthorDataService(provider.Object);

        var result = await sut.CreateAuthorAsync(input);

        Assert.False(result);
    }

    private static Mock<IServiceProvider> CreateProvider(IUnitOfWork<SyncDbContext> unitOfWork)
    {
        var provider = new Mock<IServiceProvider>();
        provider.Setup(x => x.GetService(typeof(IUnitOfWork<SyncDbContext>))).Returns(unitOfWork);
        return provider;
    }
}
