using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Web.ViewComponents;

namespace TemporalAirlinesConcept.Web.Controllers;

[ApiController]
[Route("flights")]
public class FlightController : Controller
{
    public FlightController()
    {

    }

    [HttpGet("form")]
    public async Task<IActionResult> Form()
    {
        return ViewComponent(typeof(FlightBookingFormViewComponent));
    }

    [HttpPost("form")]
    public async Task<IActionResult> Form([FromForm] FlightBookingFormViewModel model)
    {
        return ViewComponent(typeof(FlightBookingFormViewComponent), model);
    }
}
