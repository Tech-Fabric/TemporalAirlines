using Htmx;
using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Implementations.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using TemporalAirlinesConcept.Web.ViewComponents;

namespace TemporalAirlinesConcept.Web.Controllers;

[ApiController]
[Route("flights")]
public class FlightController : Controller
{
    private readonly TicketService _ticketService;

    public FlightController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("form")]
    public async Task<IActionResult> Form()
    {
        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent));
        }
        else
        {
            return View("~/Views/Home/Index.cshtml");
        }
    }

    [HttpPost("form")]
    public async Task<IActionResult> Form([FromForm] FlightBookingFormViewModel model, [FromQuery] string? selectedFlight)
    {
        if (!string.IsNullOrEmpty(selectedFlight))
        {
            model.SelectedFlight = selectedFlight;
        }

        if (model.SelectedSeats.Values.Any(v => v))
        {
            var result = await _ticketService.RequestTicketPurchaseAsync(
                new PurchaseModel()
                {
                    FlightsId = new List<string>() { model.SelectedFlight }
                }
            );
        }

        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }
        else
        {
            return View("~/Views/Home/Index.cshtml", model);
        }
    }
}
