using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Shared.Services.ServiceModels;

namespace BookShelves.Maui.Services;

internal class MauiVersionService : IVersionService
{
    public VersionInfo GetVersion()
    {
        var temp = AppInfo.Current;
        return new VersionInfo()
        {
            CurrentVersion = VersionTracking.Default.CurrentVersion.ToString(),
            CurrentBuild = VersionTracking.Default.CurrentBuild.ToString()
        };
    }
}
