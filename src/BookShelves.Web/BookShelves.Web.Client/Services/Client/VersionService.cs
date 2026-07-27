using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Shared.Services.ServiceModels;
using System.Reflection;

namespace BookShelves.Web.Client.Services.Client;

internal class VersionService : IVersionService
{
    public VersionInfo GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var version = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        return new VersionInfo()
        {
            CurrentVersion = version?.InformationalVersion ?? "NA",
            CurrentBuild = "0",
        };
    }
}
