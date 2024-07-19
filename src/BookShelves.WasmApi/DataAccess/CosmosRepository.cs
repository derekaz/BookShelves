using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CosmosRepository<T> where T : IItem, new()
{
    private readonly Container container;
    private readonly ILogger logger;

    public CosmosRepository(
        ILogger logger,
        CosmosClient cosmosDbClient,
        string databaseName,
        string containerName)
    {
        container = cosmosDbClient.GetContainer(databaseName, containerName);
        this.logger = logger;   
    }
    public async Task AddAsync(T item)
    {
        await container.CreateItemAsync(item, new PartitionKey(item.Id));
    }
    public async Task DeleteAsync(string id)
    {
        await container.DeleteItemAsync<T>(id, new PartitionKey(id));
    }
    public async Task<T?> GetAsync(string id)
    {
        try
        { 
            var response = await container.ReadItemAsync<T>(id, new PartitionKey(id));
            return response.Resource;
        }
        catch (CosmosException) //For handling item not found and other exceptions
        {
            return default;
        }
    }
    public async Task<IEnumerable<T>> GetMultipleAsync(string queryString)
    {
        var query = container.GetItemQueryIterator<T>(new QueryDefinition(queryString));
        var results = new List<T>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }
        return results;
    }
    public async Task UpdateAsync(string id, T item)
    {
        await container.UpsertItemAsync(item, new PartitionKey(id));
    }
}