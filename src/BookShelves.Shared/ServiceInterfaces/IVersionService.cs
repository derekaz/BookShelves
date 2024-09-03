using BookShelves.Shared.ServiceModels;

namespace BookShelves.Shared.ServiceInterfaces;

public interface IVersionService
{
    VersionInfo GetVersion();

}
