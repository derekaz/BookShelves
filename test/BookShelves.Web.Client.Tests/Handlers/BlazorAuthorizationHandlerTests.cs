using System.Net;
using BookShelves.Web.Client.Handlers;
using Microsoft.AspNetCore.Components;

namespace BookShelves.Web.Client.Tests.Handlers;

public sealed class BlazorAuthorizationHandlerTests
{
    [Fact]
    public async Task SendAsync_WhenUnauthorized_NavigatesToLoginWithReturnUrl()
    {
        var navigation = new TestNavigationManager("https://bookshelves.test/");
        var inner = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));
        using var sut = CreateSut(navigation, inner);

        _ = await sut.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://bookshelves.test/books"), CancellationToken.None);

        Assert.NotNull(navigation.LastUri);
        Assert.Contains("/MicrosoftIdentity/Account/Challenge?returnUrl=", navigation.LastUri!, StringComparison.Ordinal);
        Assert.Contains("returnUrl=", navigation.LastUri!, StringComparison.Ordinal);
        Assert.True(navigation.LastForceLoad);

        var expectedBase = "https://bookshelves.test/";
        Assert.StartsWith(expectedBase, navigation.LastUri!, StringComparison.Ordinal);

        Assert.DoesNotContain("account/login?returnUrl=", navigation.LastUri!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SendAsync_WhenAuthorized_DoesNotNavigate()
    {
        var navigation = new TestNavigationManager("https://bookshelves.test/");
        var inner = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        using var sut = CreateSut(navigation, inner);

        _ = await sut.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://bookshelves.test/books"), CancellationToken.None);

        Assert.Null(navigation.LastUri);
    }

    private static HttpMessageInvoker CreateSut(NavigationManager navigationManager, HttpMessageHandler inner)
    {
        var handler = new BlazorAuthorizationHandler(navigationManager)
        {
            InnerHandler = inner
        };

        return new HttpMessageInvoker(handler, disposeHandler: true);
    }

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public string? LastUri { get; private set; }

        public bool LastForceLoad { get; private set; }

        public TestNavigationManager(string baseUri)
        {
            Initialize(baseUri, baseUri);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            LastUri = uri;
            LastForceLoad = forceLoad;
            Uri = ToAbsoluteUri(uri).ToString();
        }
    }
}
