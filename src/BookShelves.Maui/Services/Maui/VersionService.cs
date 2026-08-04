using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Shared.Services.ServiceModels;
using System.Reflection;

namespace BookShelves.Maui.Services.Maui;

internal class VersionService : IVersionService
{
    public VersionInfo GetVersion()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        return new VersionInfo()
        {
            CurrentVersion = version?.InformationalVersion ?? "NA",
            CurrentBuild = VersionTracking.Default.CurrentBuild.ToString()
        };
    }
}
