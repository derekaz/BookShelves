using System.Reflection;
using BookShelves.Shared.Presentation.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.Identity.Web;
using Moq;

namespace BookShelves.Web.Tests.Services;

public sealed class BookUserActionsDataServiceTests
{
    [Fact]
    public async Task CreateBookUserActionAsync_WhenActionIsNull_ThrowsArgumentNullException()
    {
        var sut = CreateService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.CreateBookUserActionAsync(null!));
    }

    [Fact]
    public async Task UpdateBookUserActionAsync_WhenIdMissing_ThrowsArgumentException()
    {
        var sut = CreateService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<ArgumentException>(() => sut.UpdateBookUserActionAsync(new BookUserActionViewModel { Id = "" }));
    }

    [Fact]
    public async Task DeleteBookUserActionAsync_WhenIdMissing_ThrowsArgumentException()
    {
        var sut = CreateService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<ArgumentException>(() => sut.DeleteBookUserActionAsync(new BookUserActionViewModel { Id = "" }));
    }

    [Fact]
    public async Task CreateBookUserActionAsync_WhenTokenAcquisitionFails_WrapsInvalidOperationException()
    {
        var sut = CreateService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.CreateBookUserActionAsync(BookUserActionViewModel.CreateFinished("book-1", "user-1")));

        Assert.Contains("Error creating book user action.", ex.Message);
        Assert.NotNull(ex.InnerException);
    }

    [Fact]
    public async Task GetBookUserActionsAsync_WhenTokenAcquisitionFails_RethrowsMsalUiRequiredException()
    {
        var sut = CreateService(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        await Assert.ThrowsAsync<MsalUiRequiredException>(() => sut.GetBookUserActionsAsync());
    }

    [Fact]
    public void CreateClient_UsesBookUserActionsTableEndpoint()
    {
        var factory = CreateFactory(CreateTokenServiceThatThrows(new MsalUiRequiredException("mock_code", "mock ui required")));

        var client = factory.CreateClient();

        Assert.NotNull(client.BaseAddress);
        Assert.Equal("https://api.example.test/books/tables/", client.BaseAddress?.ToString());
    }

    private static BookShelves.Web.Services.Server.BookUserActionsDataService CreateService(Mock<ITokenAcquisition> tokenService)
    {
        var factory = CreateFactory(tokenService);
        var logger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Services.Server.BookUserActionsDataService>();
        return new BookShelves.Web.Services.Server.BookUserActionsDataService(factory, logger);
    }

    private static BookShelves.Web.Services.BookUserActionsDatasyncClientFactory CreateFactory(Mock<ITokenAcquisition> tokenService)
    {
        var handlerLogger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Handlers.BearerTokenHandler>();
        var handler = new BookShelves.Web.Handlers.BearerTokenHandler(tokenService.Object, handlerLogger);
        var factoryLogger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Services.BookUserActionsDatasyncClientFactory>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["BooksApi:BaseUrl"] = "https://api.example.test/books"
        }).Build();

        return new BookShelves.Web.Services.BookUserActionsDatasyncClientFactory(configuration, handler, factoryLogger);
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
