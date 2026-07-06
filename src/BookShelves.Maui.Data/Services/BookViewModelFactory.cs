using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Maui.Data.Services;

public class BookViewModelFactory : IBookFactory
{
    public BookViewModel CreateBook() => new BookViewModel();
}
