using BookShelves.Shared.Services.ServiceInterfaces;
using BookShelves.Shared.Services.ServiceModels;
using System.Reflection;

namespace BookShelves.Web.Client.Services;

internal class ClientVersionService : IVersionService
{
    public VersionInfo GetVersion()
    {
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        return new VersionInfo()
        {
            CurrentVersion = version?.InformationalVersion ?? "NA",
            CurrentBuild = "0",
        };
    }
}
