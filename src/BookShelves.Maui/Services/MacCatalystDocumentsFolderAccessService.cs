using BookShelves.Maui.Helpers;
using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Maui.Services;

public class MacCatalystDocumentsFolderAccessService : IDocumentsFolderAccessService
{
    public Task<string?> RequestAccessAsync() => FileAccessHelper.PickMacCatalystDocumentsRootAsync();
}
