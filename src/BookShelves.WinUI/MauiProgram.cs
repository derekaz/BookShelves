using BookShelves.Maui2;
using BookShelves.Maui2.Helpers;
using BookShelves.Maui2.Services;
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
