using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;

namespace BookShelves.Web.Tests.Services;

public sealed class DatasyncFactoriesTests
{
    [Fact]
    public void AuthorsFactory_MissingBaseUrl_ThrowsInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var tokenService = new Mock<ITokenAcquisition>(MockBehavior.Loose);
        var bearerLogger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Handlers.BearerTokenHandler>();
        var handler = new BookShelves.Web.Handlers.BearerTokenHandler(tokenService.Object, bearerLogger);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Services.AuthorsDatasyncClientFactory>();

        Assert.Throws<InvalidOperationException>(() => new BookShelves.Web.Services.AuthorsDatasyncClientFactory(configuration, handler, logger));
    }

    [Fact]
    public void BooksFactory_MissingBaseUrl_ThrowsInvalidOperationException()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var tokenService = new Mock<ITokenAcquisition>(MockBehavior.Loose);
        var bearerLogger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Handlers.BearerTokenHandler>();
        var handler = new BookShelves.Web.Handlers.BearerTokenHandler(tokenService.Object, bearerLogger);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Services.BooksDatasyncClientFactory>();

        Assert.Throws<InvalidOperationException>(() => new BookShelves.Web.Services.BooksDatasyncClientFactory(configuration, handler, logger));
    }

    [Fact]
    public void AuthorsFactory_CreateClient_UsesTablesEndpoint()
    {
        var configuration = CreateConfiguration("https://api.example.test/books");
        var tokenService = new Mock<ITokenAcquisition>(MockBehavior.Loose);
        var bearerLogger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Handlers.BearerTokenHandler>();
        var handler = new BookShelves.Web.Handlers.BearerTokenHandler(tokenService.Object, bearerLogger);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Services.AuthorsDatasyncClientFactory>();

        var sut = new BookShelves.Web.Services.AuthorsDatasyncClientFactory(configuration, handler, logger);

        var client = sut.CreateClient();

        Assert.NotNull(client.BaseAddress);
        Assert.Equal("https://api.example.test/books/tables/", client.BaseAddress?.ToString());
    }

    [Fact]
    public void BooksFactory_CreateClient_UsesTablesEndpoint()
    {
        var configuration = CreateConfiguration("https://api.example.test/books/");
        var tokenService = new Mock<ITokenAcquisition>(MockBehavior.Loose);
        var bearerLogger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Handlers.BearerTokenHandler>();
        var handler = new BookShelves.Web.Handlers.BearerTokenHandler(tokenService.Object, bearerLogger);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Services.BooksDatasyncClientFactory>();

        var sut = new BookShelves.Web.Services.BooksDatasyncClientFactory(configuration, handler, logger);

        var client = sut.CreateClient();

        Assert.NotNull(client.BaseAddress);
        Assert.Equal("https://api.example.test/books/tables/", client.BaseAddress?.ToString());
    }

    private static IConfiguration CreateConfiguration(string baseUrl)
        => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BooksApi:BaseUrl"] = baseUrl
        }).Build();
}
