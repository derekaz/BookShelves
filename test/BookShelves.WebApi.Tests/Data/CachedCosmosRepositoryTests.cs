using BookShelves.WebApi.BooksDataAccess;
using BookShelves.WebApi.Data;
using CommunityToolkit.Datasync.Server;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BookShelves.WebApi.Tests.Data;

public sealed class CachedCosmosRepositoryTests
{
    [Fact]
    public async Task ReadAsync_CachesById_AfterFirstRead()
    {
        var inner = new Mock<IRepository<Book>>();
        var book = new Book { Id = "book-1", Title = "Cached" };

        inner.Setup(x => x.ReadAsync("book-1", It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Book>(book));

        var sut = CreateSut(inner.Object);

        var first = await sut.ReadAsync("book-1", CancellationToken.None);
        var second = await sut.ReadAsync("book-1", CancellationToken.None);

        Assert.Same(first, second);
        inner.Verify(x => x.ReadAsync("book-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReadAsync_WhenRepositoryReturnsNull_ThrowsInvalidOperationException()
    {
        var inner = new Mock<IRepository<Book>>();
        inner.Setup(x => x.ReadAsync("book-null", It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Book>((Book)null!));

        var sut = CreateSut(inner.Object);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.ReadAsync("book-null", CancellationToken.None).AsTask());

        Assert.Contains("Repository returned null", ex.Message);
    }

    [Fact]
    public async Task AsQueryableAsync_WhenQueryIsNotCosmosOrdered_ThrowsArgumentOutOfRangeException()
    {
        var inner = new Mock<IRepository<Book>>();
        var data = new List<Book>
        {
            new() { Id = "book-a", Title = "A" },
            new() { Id = "book-b", Title = "B" }
        };

        inner.Setup(x => x.AsQueryableAsync(It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<IQueryable<Book>>(data.AsQueryable()));

        var sut = CreateSut(inner.Object);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.AsQueryableAsync(CancellationToken.None).AsTask());
    }

    [Fact]
    public async Task CreateAsync_ClearsItemCache_ForCreatedId()
    {
        var inner = new Mock<IRepository<Book>>();
        var created = new Book { Id = "book-2", Title = "New" };

        inner.Setup(x => x.ReadAsync("book-2", It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Book>(created));
        inner.Setup(x => x.CreateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var sut = CreateSut(inner.Object);

        _ = await sut.ReadAsync("book-2", CancellationToken.None);
        await sut.CreateAsync(created, CancellationToken.None);
        _ = await sut.ReadAsync("book-2", CancellationToken.None);

        inner.Verify(x => x.ReadAsync("book-2", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ReplaceAsync_ClearsItemCache_ForReplacedId()
    {
        var inner = new Mock<IRepository<Book>>();
        var item = new Book { Id = "book-r", Title = "Replace me" };

        inner.Setup(x => x.ReadAsync("book-r", It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Book>(item));
        inner.Setup(x => x.ReplaceAsync(It.IsAny<Book>(), null, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var sut = CreateSut(inner.Object);

        _ = await sut.ReadAsync("book-r", CancellationToken.None);

        await sut.ReplaceAsync(item, cancellationToken: CancellationToken.None);

        _ = await sut.ReadAsync("book-r", CancellationToken.None);

        inner.Verify(x => x.ReadAsync("book-r", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task DeleteAsync_ClearsItemCache_ForDeletedId()
    {
        var inner = new Mock<IRepository<Book>>();
        var item = new Book { Id = "book-3", Title = "Delete me" };

        inner.Setup(x => x.ReadAsync("book-3", It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<Book>(item));
        inner.Setup(x => x.DeleteAsync("book-3", null, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var sut = CreateSut(inner.Object);

        _ = await sut.ReadAsync("book-3", CancellationToken.None);
        await sut.DeleteAsync("book-3", cancellationToken: CancellationToken.None);
        _ = await sut.ReadAsync("book-3", CancellationToken.None);

        inner.Verify(x => x.ReadAsync("book-3", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private static CachedCosmosRepository<Book> CreateSut(IRepository<Book> inner)
    {
        var cache = new MemoryCache(new MemoryCacheOptions());
        return new CachedCosmosRepository<Book>(inner, cache, NullLogger<CachedCosmosRepository<Book>>.Instance);
    }
}
