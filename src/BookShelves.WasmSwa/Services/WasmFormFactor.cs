using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.WasmSwa.Services;

internal class WasmFormFactor : IFormFactor
{
    public string GetFormFactor()
    {
        return "WebAssembly-SWA";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }
}
