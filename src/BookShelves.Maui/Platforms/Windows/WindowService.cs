using BookShelves.Maui.Services;
using System.Runtime.InteropServices;

namespace BookShelves.Maui.Platforms.Windows;

enum GetAncestorFlags
{
    GetParent = 1,
    GetRoot = 2,
    GetRootOwner = 3
}

internal class WindowService : IWindowService
{
    [DllImport("user32.dll", ExactSpelling = true)]
    static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

    [DllImport("kernel32.dll")]
    static extern IntPtr GetConsoleWindow();


    public static IntPtr GetConsoleOrTerminalWindow()
    {
        var appWindow = App.Current!.Windows[0];
        var nativeWindow = appWindow.Handler.PlatformView as MauiWinUIWindow;
        var window = nativeWindow!.WindowHandle;

        // var window = (MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView;

        return window;
    }

    public Func<object> GetMainWindowHandle()
    {
        return () => GetConsoleOrTerminalWindow();
    }
}
