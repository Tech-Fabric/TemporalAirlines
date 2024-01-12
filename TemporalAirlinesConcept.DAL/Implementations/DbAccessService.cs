using Microsoft.Azure.Cosmos;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Implementations;

public class DbAccessService<T> : IDbAccessService<T> where T : class
{
    private readonly Container _container;

    public DbAccessService(CosmosClient dbClient, string databaseName, string containerName)
    {
        _container = dbClient.GetContainer(databaseName, containerName);
    }

    public async Task<T> GetItemAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, new PartitionKey(id));

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<T>> GetItemsAsync(string queryString)
    {
        var query = _container.GetItemQueryIterator<T>(new QueryDefinition(queryString));

        var results = new List<T>();

        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();

            results.AddRange(response.ToList());
        }

        return results;
    }

    public async Task AddItemAsync(T item, string id)
    {
        await _container.CreateItemAsync<T>(item, new PartitionKey(id));
    }

    public async Task UpdateItemAsync(string id, T item)
    {
        await _container.UpsertItemAsync<T>(item, new PartitionKey(id));
    }

    public async Task DeleteItemAsync(string id)
    {
        await _container.DeleteItemAsync<T>(id, new PartitionKey(id));
    }
}