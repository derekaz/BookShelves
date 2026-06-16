using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Web.Client.Services;

public class ClientFormFactor : IFormFactor
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
