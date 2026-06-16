using BookShelves.Shared.Services.ServiceInterfaces;

namespace BookShelves.Web.Services;

public class ServerFormFactor : IFormFactor
{
    public string GetFormFactor()
    {
        return "Web";
    }

    public string GetPlatform()
    {
        return Environment.OSVersion.ToString();
    }
}
