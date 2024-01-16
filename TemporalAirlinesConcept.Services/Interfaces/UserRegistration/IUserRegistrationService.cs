using TemporalAirlinesConcept.Services.Models.User;

namespace TemporalAirlinesConcept.Services.Interfaces.UserRegistration
{
    public interface IUserRegistrationService
    {
        Task RegisterUser(UserRegistrationModel registrationModel);
    }
}
