using CommunityToolkit.Datasync.Server;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace BookShelves.WebApi.Data;

public class CachedCosmosRepository<T> : IRepository<T> where T : class, ITableData
{
    readonly IRepository<T> _innerRepository;
    readonly ILogger<CachedCosmosRepository<T>> _logger;
    readonly IMemoryCache _cache;
    readonly string _cacheKeyPrefix = $"datasync_cache_{typeof(T).Name}";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

    public CachedCosmosRepository(IRepository<T> innerRepository, IMemoryCache cache, ILogger<CachedCosmosRepository<T>> logger)
    {
        _innerRepository = innerRepository;
        _logger = logger;
        _cache = cache;
    }

    // Cache the read queries safely
    public async ValueTask<IQueryable<T>> AsQueryableAsync(CancellationToken cancellationToken = default)
    {
        var materializedList = await _cache.GetOrCreateAsync($"{_cacheKeyPrefix}_list", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;

            // 1. Fetch the query definition from the toolkit's Cosmos repository
            var cosmosQuery = await _innerRepository.AsQueryableAsync(cancellationToken);

            // 2. Cast or safely adapt the query to an IOrderedQueryable to access Cosmos SDK async pagination
            if (cosmosQuery is IOrderedQueryable<T> orderedQuery)
            {
                using var iterator = orderedQuery.ToFeedIterator();
                var results = new List<T>();

                // 3. Drain the Cosmos DB feed asynchronously 
                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);
                    results.AddRange(response);
                }

                return results;
            }

            // Fallback fallback mechanism for testing/in-memory safety configurations
            return cosmosQuery.ToList();
        });

        // Return as an in-memory LINQ-to-Objects queryable
        return materializedList?.AsQueryable() ?? throw new InvalidOperationException($"Repository returned null for {typeof(T).Name}");
    }

    public async ValueTask<T> ReadAsync(string id, CancellationToken cancellationToken)
    {
        var result = await _cache.GetOrCreateAsync($"{_cacheKeyPrefix}_{id}", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
            return await _innerRepository.ReadAsync(id, cancellationToken);
        });

        return result ?? throw new InvalidOperationException($"Repository returned null for {typeof(T).Name} with ID {id}");
    }

    // Write operations MUST bypass and invalidate the cache
    public async ValueTask CreateAsync(T entity, CancellationToken cancellationToken)
    {
        await _innerRepository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Clearing cache for item with ID {Id}", entity.Id);
        ClearCache(entity.Id);
    }

    public async ValueTask ReplaceAsync(T entity, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        await _innerRepository.ReplaceAsync(entity, null, cancellationToken);
        _logger.LogInformation("Clearing cache for item with ID {Id}", entity.Id);
        ClearCache(entity.Id);
    }

    public async ValueTask DeleteAsync(string id, byte[]? version = null, CancellationToken cancellationToken = default)
    {
        await _innerRepository.DeleteAsync(id, null, cancellationToken);
        _logger.LogInformation("Clearing cache for item with ID {Id}", id);
        ClearCache(id);
    }

    private void ClearCache(string id)
    {
        _cache.Remove($"{_cacheKeyPrefix}_list");
        _cache.Remove($"{_cacheKeyPrefix}_{id}");
    }
}