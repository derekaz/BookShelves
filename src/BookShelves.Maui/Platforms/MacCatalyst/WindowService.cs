using BookShelves.Maui.Interfaces;

namespace BookShelves.Maui.Platforms.Mac;

public class WindowService : IWindowService
{
    public Func<object?>? GetMainWindowHandle()
    {
        return null;
    }
}

