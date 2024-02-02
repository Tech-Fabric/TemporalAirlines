using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Interfaces.User;
using TemporalAirlinesConcept.Services.Interfaces.UserRegistration;
using TemporalAirlinesConcept.Services.Models.UserRegistration;

namespace TemporalAirlinesConcept.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUserRegistrationService _userRegistrationService;

        public UsersController(IUserService userService, IUserRegistrationService userRegistrationService)
        {
            _userService = userService;
            _userRegistrationService = userRegistrationService;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DAL.Entities.User>>> GetUsers()
        {
            var users = await _userService.GetUsersAsync();

            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DAL.Entities.User>> GetUser(string id)
        {
            var user = await _userService.GetUserAsync(id);

            return Ok(user);
        }

        // GET: api/users/registration/{registrationId}
        [HttpGet("registration/{registrationId}")]
        public async Task<ActionResult<UserRegistrationStatus>> GetUserRegistrationStatus(string registrationId)
        {
            var user = await _userRegistrationService.GetUserRegistrationInfo(registrationId);

            return Ok(user);
        }

        // PUT: api/users/registration/{registrationId}
        [HttpPut("registration/{registrationId}")]
        public async Task<IActionResult> ConfirmUser(string registrationId)
        {
            await _userRegistrationService.ConfirmUser(registrationId);

            return Ok();
        }

        // POST: api/users/
        [HttpPost]
        public async Task<IActionResult> RegisterUser(UserRegistrationModel model)
        {
            var registrationId = await _userRegistrationService.RegisterUser(model);

            return Ok(registrationId);
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await _userService.RemoveUserAsync(id);

            return NoContent();
        }
    }
}