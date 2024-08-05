using BookShelves.Maui2.Services;

namespace BookShelves.Droid
{
    public class WindowService : IWindowService
    {
        public Func<object> GetMainWindowHandle()
        {
            return () => Platform.CurrentActivity;
        }
    }
}
