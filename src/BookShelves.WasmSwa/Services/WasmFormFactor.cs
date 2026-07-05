using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.WasmSwa.Services;

internal class WasmFormFactor(IVersionService versionService) : IFormFactor
{
    public string GetFormFactor()
    {
        return "WebAssembly-SWA";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }

    public string GetVersion()
    {
        var version = versionService.GetVersion();
        return version.CurrentVersion.ToString() + "-" + version.CurrentBuild.ToString();
    }
}
