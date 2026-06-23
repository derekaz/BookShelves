using BookShelves.Shared.Data.Interfaces;
using BookShelves.Web.Shared.Data;

namespace BookShelves.Web.Client.Services;

public class ClientBookFactory : IBookFactory
{
    public IBook CreateBook() => new Book();
}
