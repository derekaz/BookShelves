using BookShelves.Shared.Data.Interfaces;
using BookShelves.Shared.Presentation.ViewModels;

namespace BookShelves.Web.Client.Services;

public class ClientBookFactory : IBookFactory
{
    public BookViewModel CreateBook() => new BookViewModel();
}
