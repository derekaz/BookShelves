using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Web.Shared.Data;

public class BookViewModelFactory : IBookFactory
{
    public BookViewModel CreateBook() => new BookViewModel();
}
