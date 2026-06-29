using BookShelves.Shared.Data.Interfaces;

namespace BookShelves.Maui.Data.Services;

public class LocalBookFactory // : IBookFactory
{
    public IBook CreateBook() => new Models.LocalBook();
}
