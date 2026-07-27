using Android.App;
using BookShelves.Maui.Interfaces;

namespace BookShelves.Maui.Platforms.Android;

public class WindowService : IWindowService
{
    public Func<object?>? GetMainWindowHandle()
    {
        return () => Platform.CurrentActivity;
    }
}

