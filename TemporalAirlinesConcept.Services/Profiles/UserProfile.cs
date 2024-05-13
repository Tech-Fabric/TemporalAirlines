using AutoMapper;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.User;

namespace TemporalAirlinesConcept.Services.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserInputModel, User>();
    }
}