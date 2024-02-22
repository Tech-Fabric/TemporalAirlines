using AutoMapper;
using TemporalAirlinesConcept.Common.Exceptions;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Interfaces.User;
using TemporalAirlinesConcept.Services.Models.User;

namespace TemporalAirlinesConcept.Services.Implementations.User;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<DAL.Entities.User>> GetUsers()
    {
        var user = (await _unitOfWork.Repository<DAL.Entities.User>()
            .GetAll())
            .ToList();

        return user.ToList();
    }

    public async Task<DAL.Entities.User> GetUser(Guid id)
    {
        var user = await _unitOfWork.Repository<DAL.Entities.User>()
            .FindAsync(x => x.Id == id);

        return user;
    }

    public async Task<DAL.Entities.User> CreateUser(UserInputModel model)
    {
        var user = _mapper.Map<DAL.Entities.User>(model);

        _unitOfWork.Repository<DAL.Entities.User>().Insert(user);

        await _unitOfWork.SaveChangesAsync();

        return user;
    }

    public async Task RemoveUser(Guid id)
    {
        var userToRemove = await _unitOfWork.Repository<DAL.Entities.User>()
            .FindAsync(x => x.Id == id);

        if (userToRemove is null)
            throw new EntityNotFoundException("User is not found.");

        _unitOfWork.Repository<DAL.Entities.User>().Remove(userToRemove);

        await _unitOfWork.SaveChangesAsync();
    }
}