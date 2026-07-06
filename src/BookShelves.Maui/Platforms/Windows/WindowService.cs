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
        // var handle = (App.Current?.Windows[0].Handler.PlatformView as MauiWinUIWindow)?.WindowHandle ?? IntPtr.Zero;
        var window = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;

        //IntPtr consoleHandle = GetConsoleWindow();
        //IntPtr handle = GetAncestor(consoleHandle, GetAncestorFlags.GetRootOwner);
        // return handle;

        //var mauiWindow = Microsoft.Maui.Controls.Application.Current?.Windows[0];
        //var nativeWindow = mauiWindow?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
        //IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);

        return window; // Handle;
    }

    public Func<object> GetMainWindowHandle()
    {
        return () => GetConsoleOrTerminalWindow();
    }
}
