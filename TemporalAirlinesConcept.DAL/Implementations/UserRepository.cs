using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using TemporalAirlinesConcept.Common.Settings;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Implementations;

public class UserRepository : IUserRepository
{
    private readonly IDbAccessService<Entities.User> _userDbService;

    public UserRepository(CosmosClient cosmosClient, IOptions<DatabaseSettigns> options)
    {
        _userDbService = new DbAccessService<Entities.User>(cosmosClient,
            options.Value.DbName, Entities.User.Container);
    }

    public Task<Entities.User> GetUserAsync(string id)
    {
        return _userDbService.GetItemAsync(id);
    }

    public async Task<List<Entities.User>> GetUsersAsync()
    {
        var queryResult = await _userDbService.GetItemsAsync("select * from c");

        return queryResult.ToList();
    }

    public async Task<List<Entities.User>> QueryAsync(string query)
    {
        var queryResult = await _userDbService.GetItemsAsync(query);

        return queryResult.ToList();
    }

    public Task AddUserAsync(Entities.User user)
    {
        return _userDbService.AddItemAsync(user, user.Id);
    }

    public Task UpdateUserAsync(Entities.User user)
    {
        return _userDbService.UpdateItemAsync(user.Id, user);
    }

    public Task DeleteUserAsync(string id)
    {
        return _userDbService.DeleteItemAsync(id);
    }
}