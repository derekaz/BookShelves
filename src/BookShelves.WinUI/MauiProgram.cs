using BookShelves.Maui;
using BookShelves.Maui.Helpers;
using BookShelves.Maui.Services;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace BookShelves.WinUI
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.Services.AddSingleton<IWindowService, WindowService>();

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
