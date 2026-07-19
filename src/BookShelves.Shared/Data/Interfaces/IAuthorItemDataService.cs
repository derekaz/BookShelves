using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Shared.Data.Interfaces;

public interface IAuthorItemDataService //<T> where T : IBook
{
    Task<IEnumerable<AuthorItemViewModel>> GetAuthorsAsync(bool includeSoftDeleted = false);

    Task<bool> CreateAuthorAsync(AuthorItemViewModel author);

    Task<bool> UpdateAuthorAsync(AuthorItemViewModel author);

    Task<bool> DeleteAuthorAsync(AuthorItemViewModel author, bool softDelete = false);

    Task ServerSyncAsync();
}
