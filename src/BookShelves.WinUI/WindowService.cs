using System.Runtime.InteropServices;

namespace BookShelves.Maui.Services;

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
        var handle = (App.Current?.Windows[0].Handler.PlatformView as MauiWinUIWindow)?.WindowHandle ?? IntPtr.Zero;

        //IntPtr consoleHandle = GetConsoleWindow();
        //IntPtr handle = GetAncestor(consoleHandle, GetAncestorFlags.GetRootOwner);

        return handle;
    }

    public Func<object> GetMainWindowHandle()
    {
        return () => GetConsoleOrTerminalWindow();
    }
}
