using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Web.Services;

public class ServerBookFactory : IBookFactory
{
    public BookViewModel CreateBook() => new BookViewModel();
}
