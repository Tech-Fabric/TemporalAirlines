using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Api.Models.Base;
using TemporalAirlinesConcept.Api.Models.Users;
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
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IUserRegistrationService userRegistrationService, IMapper mapper)
        {
            _userService = userService;
            _userRegistrationService = userRegistrationService;
            _mapper = mapper;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<List<UserResponse>>> GetUsers()
        {
            var users = await _userService.GetUsers();
            var usersResponse = _mapper.Map<List<UserResponse>>(users);

            return Ok(usersResponse);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponse>> GetUser(string id)
        {
            var user = await _userService.GetUser(id);
            var userResponse = _mapper.Map<UserResponse>(user);

            return Ok(userResponse);
        }

        // GET: api/users/registration/{registrationId}
        [HttpGet("registration/{registrationId}")]
        public async Task<ActionResult<UserRegistrationStatusResponse>> GetUserRegistrationStatus(string registrationId)
        {
            var userRegistrationStatus = await _userRegistrationService.GetUserRegistrationInfo(registrationId);
            var userRegistrationStatusResponse = _mapper.Map<UserRegistrationStatusResponse>(userRegistrationStatus);

            return Ok(userRegistrationStatusResponse);
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
        public async Task<ActionResult<IdResponse>> RegisterUser(UserRegistrationRequest userRegistrationRequest)
        {
            var userRegistration = _mapper.Map<UserRegistrationModel>(userRegistrationRequest);
            var registrationId = await _userRegistrationService.RegisterUser(userRegistration);

            var registrationResponse = new IdResponse
            {
                Id = registrationId
            };

            return Ok(registrationResponse);
        }

        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await _userService.RemoveUser(id);

            return NoContent();
        }
    }
}