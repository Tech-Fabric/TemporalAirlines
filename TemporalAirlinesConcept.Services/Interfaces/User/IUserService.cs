using TemporalAirlinesConcept.Services.Models.User;

namespace TemporalAirlinesConcept.Services.Interfaces.User;

public interface IUserService
{
    public Task<List<DAL.Entities.User>> GetUsers();

    public Task<DAL.Entities.User> GetUser(Guid id);

    public Task<DAL.Entities.User> CreateUser(UserInputModel model);

    public Task RemoveUser(Guid id);
}