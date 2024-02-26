using Htmx;
using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using TemporalAirlinesConcept.Web.ViewComponents.FlightBookingForm;

namespace TemporalAirlinesConcept.Web.Controllers;

[Route("flights")]
public class FlightController : Controller
{
    private readonly ITicketService _ticketService;

    public FlightController(ITicketService ticketService)
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

    [HttpGet("{SelectedFlight}")]
    public async Task<IActionResult> Flight([FromRoute] Guid? selectedFlight)
    {
        FlightBookingFormViewModel model = new FlightBookingFormViewModel();

        return await Flight(model, selectedFlight);
    }

    [HttpPost("{SelectedFlight}")]
    public async Task<IActionResult> Flight([FromForm] FlightBookingFormViewModel model, [FromRoute] Guid? selectedFlight)
    {
        if (selectedFlight is not null)
        {
            model.SelectedFlight = selectedFlight;
        }

        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }

        return View("~/Views/Flight/Index.cshtml", model);
    }

    [HttpPost("{SelectedFlight}/purchase")]
    public async Task<IActionResult> Purchase(
        [FromForm] FlightBookingFormViewModel model,
        [FromRoute] Guid? selectedFlight
    )
    {
        if (selectedFlight is null)
        {
            model.SelectedFlight = selectedFlight;
        }

        if (!string.IsNullOrEmpty(model.CreditCardDetails?.CardNumber))
        {
            model.PurchaseId = await _ticketService.StartTicketPurchase(
                new PurchaseModel
                {
                    FlightId = model.SelectedFlight.Value,
                    NumberOfTickets = model.NumberOfSeats
                }
            );
        }

        if (!Request.IsHtmx())
        {
            return View("~/Views/Flight/Index.cshtml", model);
        }

        if (!string.IsNullOrEmpty(model.PurchaseId))
        {
            return Redirect($"/purchase/{model.PurchaseId}");
        }

        return ViewComponent(typeof(FlightBookingFormViewComponent), model);
    }
}
