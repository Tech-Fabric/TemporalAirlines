using TemporalAirlinesConcept.Services.Models.User;

namespace TemporalAirlinesConcept.Services.Interfaces.User;

public interface IUserService
{
    public Task<List<DAL.Entities.User>> GetUsersAsync();

    public Task<DAL.Entities.User> GetUserAsync(string id);

    public Task<DAL.Entities.User> CreateUserAsync(UserInputModel model);

    public Task RemoveUserAsync(string id);
}