using BookShelves.Shared.Services;
using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Shared.Tests.Services;

public sealed class DocumentsFolderAccessServiceTests
{
    [Fact]
    public async Task RequestAccessAsync_ReturnsNull_WhenNoPlatformAccessFlowExists()
    {
        IDocumentsFolderAccessService sut = new DocumentsFolderAccessService();

        var result = await sut.RequestAccessAsync();

        Assert.Null(result);
    }
}
