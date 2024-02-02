using TemporalAirlinesConcept.Services.Models.UserRegistration;

namespace TemporalAirlinesConcept.Services.Interfaces.UserRegistration;

public interface IUserRegistrationService
{
    Task<string> RegisterUser(UserRegistrationModel registrationModel);

    Task<UserRegistrationStatus> GetUserRegistrationInfo(string registrationId);
    
    Task ConfirmUser(string registrationId);
}

