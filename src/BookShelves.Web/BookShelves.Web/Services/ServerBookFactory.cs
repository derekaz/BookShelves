using BookShelves.Shared.Data.Interfaces;
using BookShelves.Web.Shared.Data;

namespace BookShelves.Web.Services;

public class ServerBookFactory : IBookFactory
{
    public IBook CreateBook() => new Book();
}
