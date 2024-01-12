using AutoMapper;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Interfaces.User;
using TemporalAirlinesConcept.Services.Models.User;

namespace TemporalAirlinesConcept.Services.Implementations.User;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public Task<List<DAL.Entities.User>> GetUsersAsync() =>
        _userRepository.GetUsersAsync();

    public Task<DAL.Entities.User> GetUserAsync(string id) =>
        _userRepository.GetUserAsync(id);

    public async Task<DAL.Entities.User> CreateUserAsync(UserInputModel model)
    {
        var user = _mapper.Map<DAL.Entities.User>(model);

        await _userRepository.AddUserAsync(user);

        return user;
    }

    public Task RemoveUserAsync(string id) =>
        _userRepository.DeleteUserAsync(id);
}