using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Interfaces.User;
using TemporalAirlinesConcept.Services.Models.User;

namespace TemporalAirlinesConcept.Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
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

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<DAL.Entities.User>> CreateUser(UserInputModel model)
        {
            var createdUser = await _userService.CreateUserAsync(model);

            return CreatedAtAction(nameof(GetUser), new { id = createdUser.Id }, createdUser);
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
