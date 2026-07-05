using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Web.Client.Services;

internal class ClientFormFactor(IVersionService versionService) : IFormFactor
{
    public string GetFormFactor()
    {
        return "WebAssembly";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }

    public string GetVersion()
    {
        var version = versionService.GetVersion();
        return version.CurrentVersion.ToString(); // + " - " + version.CurrentBuild.ToString();
    }
}
