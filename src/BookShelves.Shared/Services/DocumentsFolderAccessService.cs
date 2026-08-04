using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Shared.Services;

public class DocumentsFolderAccessService : IDocumentsFolderAccessService
{
    public Task<string?> RequestAccessAsync() => Task.FromResult<string?>(null);
}
