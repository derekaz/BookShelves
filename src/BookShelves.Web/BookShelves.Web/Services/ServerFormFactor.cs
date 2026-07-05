using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Web.Services;

internal class ServerFormFactor(IVersionService versionService) : IFormFactor
{
    public string GetFormFactor()
    {
        return "Web";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }

    public string GetVersion()
    {
        var version = versionService.GetVersion();
        return version.CurrentVersion.ToString() + " - " + version.CurrentBuild.ToString();
    }
}
