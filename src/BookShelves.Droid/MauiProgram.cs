using BookShelves.Maui;
using BookShelves.Maui.Services;
using Microsoft.Identity.Client;
using Microsoft.Maui.LifecycleEvents;

namespace BookShelves.Droid
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.Services.AddSingleton<IWindowService, WindowService>();

            builder.ConfigureLifecycleEvents(events =>
            {
#if ANDROID
                events.AddAndroid(platform =>
                {
                    platform.OnActivityResult((activity, rc, result, data) =>
                    {
                        AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(rc, result, data);
                    });
                });
#endif
            });

            builder
                .UseSharedMauiApp();

            return builder.Build();
        }
    }
}
