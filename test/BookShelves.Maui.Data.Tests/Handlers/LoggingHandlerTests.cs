using System.Net;
using BookShelves.Maui.Data.Handlers;

namespace BookShelves.Maui.Data.Tests.Handlers;

public sealed class LoggingHandlerTests
{
    [Fact]
    public async Task SendAsync_ForwardsRequestAndReturnsInnerResponse()
    {
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            ReasonPhrase = "Accepted"
        };

        HttpRequestMessage? capturedRequest = null;
        var inner = new StubHttpMessageHandler(request =>
        {
            capturedRequest = request;
            return expectedResponse;
        });

        var client = new HttpClient(new LoggingHandler(inner))
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var response = await client.PostAsync("weatherforecast", new StringContent("payload"));

        Assert.Same(expectedResponse, response);
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal(new Uri("https://example.test/weatherforecast"), capturedRequest.RequestUri);
    }

    private sealed class StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(responder(request));
    }
}
