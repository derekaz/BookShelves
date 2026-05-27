using BookShelves.Shared.Services.ServiceModels;

namespace BookShelves.Shared.Services.ServiceInterfaces;

public interface IVersionService
{
    VersionInfo GetVersion();

}
