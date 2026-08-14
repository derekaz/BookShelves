using BookShelves.Maui.Data.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace BookShelves.Maui.Data.Tests.Infrastructure;

public sealed class UnitOfWorkTests
{
    [Fact]
    public async Task GetRepository_ReturnsSameInstance_ForSameEntityType()
    {
        await using var sut = CreateSut();

        var first = sut.GetRepository<TestEntity>();
        var second = sut.GetRepository<TestEntity>();

        Assert.Same(first, second);
    }

    [Fact]
    public async Task SaveChangesAsync_PersistsEntity_ForSubsequentQuery()
    {
        await using var sut = CreateSut();
        var repository = sut.GetRepository<TestEntity>();

        await repository.AddAsync(new TestEntity { Name = "First" });
        await sut.SaveChangesAsync();

        var allItems = await repository.GetAllAsync();

        Assert.Contains(allItems, item => item.Name == "First");
    }

    private static UnitOfWork<TestDbContext> CreateSut()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var factory = new TestDbContextFactory(options);
        return new UnitOfWork<TestDbContext>(factory);
    }

    private sealed class TestDbContextFactory(DbContextOptions<TestDbContext> options) : IDbContextFactory<TestDbContext>
    {
        public TestDbContext CreateDbContext() => new(options);
    }

    private sealed class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
    {
        public DbSet<TestEntity> Entities => Set<TestEntity>();
    }

    private sealed class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
