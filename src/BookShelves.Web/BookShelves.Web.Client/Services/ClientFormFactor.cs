using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Web.Client.Services;

internal class ClientFormFactor : IFormFactor
{
    public string GetFormFactor()
    {
        return "WebAssembly";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }
}
