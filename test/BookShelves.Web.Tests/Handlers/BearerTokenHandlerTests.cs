using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;

namespace BookShelves.Web.Tests.Handlers;

public sealed class BearerTokenHandlerTests
{
    [Fact]
    public async Task SendAsync_WhenTokenAvailable_SetsBearerAuthorizationHeader()
    {
        var tokenService = new Mock<ITokenAcquisition>(MockBehavior.Strict);
        tokenService
            .Setup(x => x.GetAccessTokenForUserAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal?>(),
                It.IsAny<TokenAcquisitionOptions?>()))
            .ReturnsAsync("token-123");

        var logger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Handlers.BearerTokenHandler>();
        var sut = new TestableBearerTokenHandler(tokenService.Object, logger)
        {
            InnerHandler = new CaptureHandler()
        };

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/books");
        using var response = await sut.InvokeAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization?.Scheme);
        Assert.Equal("token-123", request.Headers.Authorization?.Parameter);
    }

    [Fact]
    public async Task SendAsync_WhenTokenIsWhitespace_DoesNotSetAuthorizationHeader()
    {
        var tokenService = new Mock<ITokenAcquisition>(MockBehavior.Strict);
        tokenService
            .Setup(x => x.GetAccessTokenForUserAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<System.Security.Claims.ClaimsPrincipal?>(),
                It.IsAny<TokenAcquisitionOptions?>()))
            .ReturnsAsync(" ");

        var logger = LoggerFactory.Create(_ => { }).CreateLogger<BookShelves.Web.Handlers.BearerTokenHandler>();
        var sut = new TestableBearerTokenHandler(tokenService.Object, logger)
        {
            InnerHandler = new CaptureHandler()
        };

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/books");
        using var response = await sut.InvokeAsync(request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(request.Headers.Authorization);
    }

    private sealed class TestableBearerTokenHandler(
        ITokenAcquisition tokenService,
        ILogger<BookShelves.Web.Handlers.BearerTokenHandler> logger)
        : BookShelves.Web.Handlers.BearerTokenHandler(tokenService, logger)
    {
        public Task<HttpResponseMessage> InvokeAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => SendAsync(request, cancellationToken);
    }

    private sealed class CaptureHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("ok")
            });
    }
}
