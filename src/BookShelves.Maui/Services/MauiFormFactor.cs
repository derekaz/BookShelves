using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Maui.Services;

internal class MauiFormFactor(IVersionService versionService) : IFormFactor
{
    public string GetFormFactor()
    {
        return DeviceInfo.Idiom.ToString();
    }

    public string GetPlatform()
    {
        return DeviceInfo.Platform.ToString() + " - " + DeviceInfo.VersionString;
    }

    public string GetVersion()
    {
        var version = versionService.GetVersion();
        return version.CurrentVersion.ToString(); // + " - " + version.CurrentBuild.ToString();
    }
}
