using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Shared.Services.ServiceModels;

namespace BookShelves.Maui.Services.Maui;

internal class VersionService : IVersionService
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
