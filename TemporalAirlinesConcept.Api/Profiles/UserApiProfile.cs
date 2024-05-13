using AutoMapper;
using TemporalAirlinesConcept.Api.Models.Users;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.UserRegistration;
using UserResponse = TemporalAirlinesConcept.Api.Models.Users.UserResponse;

namespace TemporalAirlinesConcept.Api.Profiles;

public class UserApiProfile : Profile
{
    public UserApiProfile()
    {
        CreateMap<User, UserResponse>();

        CreateMap<UserRegistrationRequest, UserRegistrationModel>();

        CreateMap<UserRegistrationStatus, UserRegistrationStatusResponse>()
            .ForMember(x => x.CreatedUser, x => x.MapFrom(y => y.CreatedUser));
    }
}