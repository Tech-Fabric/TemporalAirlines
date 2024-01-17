using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using TemporalAirlinesConcept.Common.Settings;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Implementations;

public class UserRepository : DbAccessService<Entities.User>, IUserRepository
{
    public UserRepository(CosmosClient cosmosClient, IOptions<DatabaseSettings> options)
        : base(cosmosClient, options.Value.DbName, Entities.User.Container)
    {
    }

    public Task<Entities.User> GetUserAsync(string id)
    {
        return GetItemAsync(id);
    }

    public async Task<List<Entities.User>> GetUsersAsync()
    {
        var queryResult = await GetItemsAsync("select * from c");

        return queryResult.ToList();
    }

    public async Task<List<Entities.User>> QueryAsync(string query)
    {
        var queryResult = await GetItemsAsync(query);

        return queryResult.ToList();
    }

    public Task AddUserAsync(Entities.User user)
    {
        return AddItemAsync(user, user.Id);
    }

    public Task UpdateUserAsync(Entities.User user)
    {
        return UpdateItemAsync(user.Id, user);
    }

    public Task DeleteUserAsync(string id)
    {
        return DeleteItemAsync(id);
    }
}