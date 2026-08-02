namespace BookShelves.Shared.Services.ServiceInterfaces;

public interface IDocumentsFolderAccessService
{
    Task<string?> RequestAccessAsync();
}
