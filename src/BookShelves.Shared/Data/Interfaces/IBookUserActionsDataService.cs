using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Shared.Data.Interfaces;

public interface IBookUserActionsDataService
{
    Task<IEnumerable<BookUserActionViewModel>> GetBookUserActionsAsync(bool includeSoftDeleted = false);

    Task<bool> CreateBookUserActionAsync(BookUserActionViewModel action);

    Task<bool> UpdateBookUserActionAsync(BookUserActionViewModel action);

    Task<bool> DeleteBookUserActionAsync(BookUserActionViewModel action);
}
