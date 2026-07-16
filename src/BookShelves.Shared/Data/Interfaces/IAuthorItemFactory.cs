using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Shared.Data.Interfaces;

public interface IBookFactory
{
    BookViewModel CreateBook();
}
