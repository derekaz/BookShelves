namespace BookShelves.Maui.Services;

public interface IWindowService
{
    public Func<object?>? GetMainWindowHandle();
}
