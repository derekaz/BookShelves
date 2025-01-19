using BookShelves.Maui.Services;

namespace BookShelves.Maui.Platforms.IOS;

public class WindowService : IWindowService
{
    public Func<object?>? GetMainWindowHandle()
    {
        return null;
    }
}

