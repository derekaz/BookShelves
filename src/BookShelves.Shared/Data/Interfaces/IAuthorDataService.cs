using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Shared.Data.Interfaces;

public interface IAuthorDataService
{
    Task<IEnumerable<AuthorViewModel>> GetAuthorsAsync(bool includeSoftDeleted = false);

    Task<bool> CreateAuthorAsync(AuthorViewModel author);

    Task<bool> UpdateAuthorAsync(AuthorViewModel author);

    Task<bool> DeleteAuthorAsync(AuthorViewModel author);
}
