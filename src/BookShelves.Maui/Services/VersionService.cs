using BookShelves.Shared.ServiceInterfaces;
using BookShelves.Shared.ServiceModels;

namespace BookShelves.Maui.Services;

internal class VersionService : IVersionService
{
    public VersionInfo GetVersion()
    {
        return new VersionInfo()
        {
            CurrentVersion = VersionTracking.Default.CurrentVersion.ToString(),
            CurrentBuild = VersionTracking.Default.CurrentBuild.ToString()
        };
    }
}
