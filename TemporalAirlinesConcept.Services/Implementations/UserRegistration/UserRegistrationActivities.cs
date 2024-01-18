using TemporalAirlinesConcept.Services.Interfaces.User;
using TemporalAirlinesConcept.Services.Models.User;
using TemporalAirlinesConcept.Services.Models.UserRegistration;
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
        public Task SendConfirmationCode()
        {
            //throw new Exception("Error While Sending Confirmation Code");

            return Task.CompletedTask;
        }

        [Activity]
        public async Task<DAL.Entities.User> CreateUser(UserRegistrationModel registrationModel)
        {
            var user = await _userService.CreateUserAsync(new UserInputModel
            {
                Email = registrationModel.Email,
                Name = registrationModel.Name,
                Role = "User"
            });

            return user;
        }
    }
}
