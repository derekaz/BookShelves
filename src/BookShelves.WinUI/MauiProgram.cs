using BookShelves.Maui;
using Microsoft.Extensions.Configuration;
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
