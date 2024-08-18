using BookShelves.Shared.DataInterfaces;
using System.Reflection;

namespace BookShelves.WasmSwa.Services;

internal class VersionService : IVersionService
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
