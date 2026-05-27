using BookShelves.Shared.Data.Interfaces;

namespace BookShelves.Web.Shared.Data;

public class BookFactory : IBookFactory
{
    public IBook CreateBook() => new Book();
}
