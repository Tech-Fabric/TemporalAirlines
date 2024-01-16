using Microsoft.Azure.Cosmos;

namespace TemporalAirlinesConcept.DAL.Implementations;

public class DbAccessService<T> where T : class
{
    private Container _container;
    private DatabaseResponse _database;

    private readonly CosmosClient _dbClient;
    private readonly string _databaseName;
    private readonly string _containerName;

    public DbAccessService(CosmosClient dbClient, string databaseName, string containerName)
    {
        _dbClient = dbClient;
        _databaseName = databaseName;
        _containerName = containerName;
    }

    public async Task<T> GetItemAsync(string id)
    {
        try
        {
            var container = await GetContainer();
            var response = await container.ReadItemAsync<T>(id, new PartitionKey(id));

            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<T>> GetItemsAsync(string queryString)
    {
        var container = await GetContainer();
        var query = container.GetItemQueryIterator<T>(new QueryDefinition(queryString));

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
        var container = await GetContainer();
        await container.CreateItemAsync<T>(item, new PartitionKey(id));
    }

    public async Task UpdateItemAsync(string id, T item)
    {
        var container = await GetContainer();

        await container.UpsertItemAsync<T>(item, new PartitionKey(id));
    }

    public async Task DeleteItemAsync(string id)
    {
        var container = await GetContainer();

        await container.DeleteItemAsync<T>(id, new PartitionKey(id));
    }

    private async Task<DatabaseResponse> GetDatabase()
    {
        if (_database is null)
            _database = await _dbClient.CreateDatabaseIfNotExistsAsync(_databaseName);

        return _database;
    }

    private async Task<Container> GetContainer()
    {
        var database = await GetDatabase();

        if (_container is null)
            _container = await database.Database.CreateContainerIfNotExistsAsync(
                id: _containerName,
                partitionKeyPath: "/id");

        return _container;
    }
}