using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Maui.Services;

internal class MauiFormFactor : IFormFactor
{
    public string GetFormFactor()
    {
        return DeviceInfo.Idiom.ToString();
    }

    public string GetPlatform()
    {
        return DeviceInfo.Platform.ToString() + " - " + DeviceInfo.VersionString;
    }
}
