using BookShelves.Maui.Services;

namespace BookShelves.Maui.Platforms.Mac;

public class WindowService : IWindowService
{
    public Func<object?>? GetMainWindowHandle()
    {
        return null;
    }
}

