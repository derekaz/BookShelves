using BookShelves.Maui.Data.Infrastructure;
using BookShelves.Maui.Data.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BookShelves.Maui.Data.Tests.Infrastructure;

public sealed class SyncDbContextInitializerTests
{
    [Fact]
    public void Initialize_DoesNotThrow_WhenMigrationSucceeds()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        using var context = CreateSqliteContext(connection);
        var logger = new Mock<ILogger<SyncDbContextInitializer>>();
        var sut = new SyncDbContextInitializer(context, logger.Object);

        var ex = Record.Exception(() => sut.Initialize());

        Assert.Null(ex);
    }

    [Fact]
    public void Initialize_Throws_WhenMigrationFails()
    {
        using var context = CreateInMemoryContext();
        var logger = new Mock<ILogger<SyncDbContextInitializer>>();
        var sut = new SyncDbContextInitializer(context, logger.Object);

        Assert.ThrowsAny<Exception>(() => sut.Initialize());
    }

    [Fact]
    public async Task InitializeAsync_DoesNotThrow_WhenMigrationSucceeds()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateSqliteContext(connection);
        var logger = new Mock<ILogger<SyncDbContextInitializer>>();
        var sut = new SyncDbContextInitializer(context, logger.Object);

        var ex = await Record.ExceptionAsync(() => sut.InitializeAsync(CancellationToken.None));

        Assert.Null(ex);
    }

    [Fact]
    public async Task InitializeAsync_Throws_WhenMigrationFails()
    {
        await using var context = CreateInMemoryContext();
        var logger = new Mock<ILogger<SyncDbContextInitializer>>();
        var sut = new SyncDbContextInitializer(context, logger.Object);

        await Assert.ThrowsAnyAsync<Exception>(() => sut.InitializeAsync(CancellationToken.None));
    }

    private static SyncDbContext CreateSqliteContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<SyncDbContext>()
            .UseSqlite(connection)
            .Options;

        var contextLogger = new Mock<ILogger<SyncDbContext>>();
        var syncApiClient = new Mock<ISyncApiClient>();
        syncApiClient.Setup(x => x.HttpClient).Returns(new HttpClient { BaseAddress = new Uri("https://example.test/") });

        return new SyncDbContext(options, contextLogger.Object, syncApiClient.Object);
    }

    private static SyncDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<SyncDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        var contextLogger = new Mock<ILogger<SyncDbContext>>();
        var syncApiClient = new Mock<ISyncApiClient>();
        syncApiClient.Setup(x => x.HttpClient).Returns(new HttpClient { BaseAddress = new Uri("https://example.test/") });

        return new SyncDbContext(options, contextLogger.Object, syncApiClient.Object);
    }
}
