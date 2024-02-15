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

    public async Task<List<DAL.Entities.User>> GetUsers()
    {
        var user = await _userRepository.GetUsersAsync();

        return user;
    }

    public async Task<DAL.Entities.User> GetUser(string id)
    {
        var user = await _userRepository.GetUserAsync(id);

        return user;
    }

    public async Task<DAL.Entities.User> CreateUser(UserInputModel model)
    {
        var user = _mapper.Map<DAL.Entities.User>(model);

        await _userRepository.AddUserAsync(user);

        return user;
    }

    public async Task RemoveUser(string id)
    {
        await _userRepository.DeleteUserAsync(id);
    }
}