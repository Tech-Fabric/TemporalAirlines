namespace TemporalAirlinesConcept.DAL.Interfaces;

public interface IUserRepository
{
    Task<Entities.User> GetUserAsync(string id);

    Task<List<Entities.User>> GetUsersAsync();

    Task<List<Entities.User>> QueryAsync(string query);

    Task AddUserAsync(Entities.User user);

    Task UpdateUserAsync(Entities.User user);

    Task DeleteUserAsync(string id);
}