using Microsoft.AspNetCore.Mvc;

namespace TemporalAirlinesConcept.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Ok-ok");
        }
    }
}
