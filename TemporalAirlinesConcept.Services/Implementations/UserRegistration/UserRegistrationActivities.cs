using TemporalAirlinesConcept.Services.Interfaces.User;
using TemporalAirlinesConcept.Services.Models.User;
using Temporalio.Activities;

namespace TemporalAirlinesConcept.Services.Implementations.UserRegistration
{
    public class UserRegistrationActivities
    {
        private readonly IUserService _userService;

        public UserRegistrationActivities(IUserService userService)
        {
            _userService = userService;
        }

        [Activity]
        public async Task SendConfirmationCode()
        {
            await Task.Delay(TimeSpan.FromSeconds(25));
        }

        [Activity]
        public async Task SendErrorConfirmationCode()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            throw new Exception("Erro While Sending Code");
        }

        [Activity]
        public async Task CreateUser(UserRegistrationModel registrationModel)
        {
            await _userService.CreateUserAsync(new UserInputModel
            {
                Email = registrationModel.Email,
                Name = registrationModel.Name,
                Role = "User"
            });
        }
    }
}
