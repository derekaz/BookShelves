using BookShelves.Maui;
using System.Diagnostics;

namespace BookShelves.WinUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            try
            {
                builder
                    .UseSharedMauiApp();
            }
            catch (Exception e) 
            {
                Debug.WriteLine(e);
            }

            return builder.Build();
        }
    }
}
