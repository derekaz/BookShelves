using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Moq;

namespace BookShelves.Web.Tests.Services;

public sealed class ServerDatasyncServiceTests
{
    [Fact]
    public async Task AuthorsDataService_CreateAuthorAsync_WhenTokenAcquisitionFails_WrapsInvalidOperationException()
    {
        var sut = CreateAuthorsService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateAuthorAsync(new AuthorViewModel { Name = "Ada" }));

        Assert.Contains("Error creating author.", ex.Message);
        Assert.IsType<MsalUiRequiredException>(ex.InnerException);
    }

    [Fact]
    public async Task AuthorsDataService_GetAuthorsAsync_WhenTokenAcquisitionFails_RethrowsMsalUiRequiredException()
    {
        var sut = CreateAuthorsService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<MsalUiRequiredException>(() => sut.GetAuthorsAsync());
    }

    [Fact]
    public async Task AuthorsDataService_GetAuthorsAsync_WhenUnexpectedException_RethrowsOriginalException()
    {
        var sut = CreateAuthorsService(CreateTokenServiceThatThrows(new InvalidOperationException("token boom")));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetAuthorsAsync());

        Assert.Equal("token boom", ex.Message);
    }

    [Fact]
    public async Task AuthorsDataService_UpdateAuthorAsync_WhenAuthorIsNull_ThrowsArgumentNullException()
    {
        var sut = CreateAuthorsService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateAuthorAsync(null!));
    }

    [Fact]
    public async Task AuthorsDataService_UpdateAuthorAsync_WhenIdMissing_ThrowsArgumentException()
    {
        var sut = CreateAuthorsService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateAuthorAsync(new AuthorViewModel { Id = "", Name = "Ada" }));
    }

    [Fact]
    public async Task AuthorsDataService_DeleteAuthorAsync_WhenAuthorIsNull_ThrowsArgumentNullException()
    {
        var sut = CreateAuthorsService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.DeleteAuthorAsync(null!));
    }

    [Fact]
    public async Task AuthorsDataService_DeleteAuthorAsync_WhenIdMissing_ThrowsArgumentException()
    {
        var sut = CreateAuthorsService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<ArgumentException>(() => sut.DeleteAuthorAsync(new AuthorViewModel { Id = "" }));
    }

    [Fact]
    public async Task BooksDataService_CreateBookAsync_WhenTokenAcquisitionFails_WrapsInvalidOperationException()
    {
        var sut = CreateBooksService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateBookAsync(new BookViewModel { Title = "Book A" }));

        Assert.Contains("Error creating book.", ex.Message);
        Assert.IsType<MsalUiRequiredException>(ex.InnerException);
    }

    [Fact]
    public async Task BooksDataService_GetBooksAsync_WhenTokenAcquisitionFails_RethrowsMsalUiRequiredException()
    {
        var sut = CreateBooksService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<MsalUiRequiredException>(() => sut.GetBooksAsync());
    }

    [Fact]
    public async Task BooksDataService_GetBooksAsync_WhenUnexpectedException_RethrowsOriginalException()
    {
        var sut = CreateBooksService(CreateTokenServiceThatThrows(new InvalidOperationException("token boom")));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetBooksAsync());

        Assert.Equal("token boom", ex.Message);
    }

    [Fact]
    public async Task BooksDataService_UpdateBookAsync_WhenBookIsNull_ThrowsArgumentNullException()
    {
        var sut = CreateBooksService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateBookAsync(null!));
    }

    [Fact]
    public async Task BooksDataService_UpdateBookAsync_WhenIdMissing_ThrowsArgumentException()
    {
        var sut = CreateBooksService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateBookAsync(new BookViewModel { Id = "", Title = "Book A" }));
    }

    [Fact]
    public async Task BooksDataService_DeleteBookAsync_WhenBookIsNull_ThrowsArgumentNullException()
    {
        var sut = CreateBooksService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.DeleteBookAsync(null!));
    }

    [Fact]
    public async Task BooksDataService_DeleteBookAsync_WhenIdMissing_ThrowsArgumentException()
    {
        var sut = CreateBooksService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<ArgumentException>(() => sut.DeleteBookAsync(new BookViewModel { Id = "" }));
    }

    private static BookShelves.Web.Services.Server.AuthorsDataService CreateAuthorsService(Mock<ITokenAcquisition> tokenService)
    {
        var factory = CreateAuthorsFactory(tokenService);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Services.Server.AuthorsDataService>();
        return new BookShelves.Web.Services.Server.AuthorsDataService(factory, logger);
    }

    private static BookShelves.Web.Services.Server.BooksDataService CreateBooksService(Mock<ITokenAcquisition> tokenService)
    {
        var factory = CreateBooksFactory(tokenService);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Services.Server.BooksDataService>();
        return new BookShelves.Web.Services.Server.BooksDataService(factory, logger);
    }

    private static BookShelves.Web.Services.AuthorsDatasyncClientFactory CreateAuthorsFactory(Mock<ITokenAcquisition> tokenService)
    {
        var handlerLogger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Handlers.BearerTokenHandler>();
        var handler = new BookShelves.Web.Handlers.BearerTokenHandler(tokenService.Object, handlerLogger);
        var factoryLogger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Services.AuthorsDatasyncClientFactory>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BooksApi:BaseUrl"] = "https://api.example.test/books"
        }).Build();

        return new BookShelves.Web.Services.AuthorsDatasyncClientFactory(configuration, handler, factoryLogger);
    }

    private static BookShelves.Web.Services.BooksDatasyncClientFactory CreateBooksFactory(Mock<ITokenAcquisition> tokenService)
    {
        var handlerLogger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Handlers.BearerTokenHandler>();
        var handler = new BookShelves.Web.Handlers.BearerTokenHandler(tokenService.Object, handlerLogger);
        var factoryLogger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Services.BooksDatasyncClientFactory>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BooksApi:BaseUrl"] = "https://api.example.test/books"
        }).Build();

        return new BookShelves.Web.Services.BooksDatasyncClientFactory(configuration, handler, factoryLogger);
    }

    private static Mock<ITokenAcquisition> CreateTokenServiceThatThrows(Exception exception)
    {
        var tokenService = new Mock<ITokenAcquisition>(MockBehavior.Strict);
        tokenService
            .Setup(x => x.GetAccessTokenForUserAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal?>(),
                It.IsAny<TokenAcquisitionOptions?>()))
            .ThrowsAsync(exception);

        return tokenService;
    }
}
