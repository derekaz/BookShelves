using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookShelves.Shared;

public static class RazorProgramExtensions
{
    public static IServiceCollection AddRazorClassLibraryServices(this IServiceCollection serviceCollection)
    {
        return serviceCollection;
    }

    public static IServiceCollection AddRazorClassLibraryServices(this IServiceCollection serviceCollection, IConfiguration config)
    {
        return serviceCollection;
    }
}
