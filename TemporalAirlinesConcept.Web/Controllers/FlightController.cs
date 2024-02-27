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

    [HttpGet]
    public async Task<IActionResult> Form()
    {
        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent));
        }
        else
        {
            return View("~/Views/Flight/Index.cshtml");
        }
    }

    [HttpPost]
    public IActionResult Form([FromForm] FlightBookingFormViewModel model)
    {
        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }
        else
        {
            return View("~/Views/Flight/Index.cshtml", model);
        }
    }

    [HttpGet("{SelectedFlight}")]
    public IActionResult Flight([FromRoute] string? selectedFlight)
    {
        FlightBookingFormViewModel model = new FlightBookingFormViewModel();

        return Flight(model, selectedFlight);
    }

    [HttpPost("{SelectedFlight}")]
    public IActionResult Flight([FromForm] FlightBookingFormViewModel model, [FromRoute] string? selectedFlight)
    {
        if (!string.IsNullOrEmpty(selectedFlight))
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
        [FromRoute] string? selectedFlight
    )
    {
        if (!string.IsNullOrEmpty(selectedFlight))
        {
            model.SelectedFlight = selectedFlight;
        }

        if (!string.IsNullOrEmpty(model.CreditCardDetails?.CardNumber))
        {
            model.PurchaseId = await _ticketService.StartTicketPurchase(
                new PurchaseModel
                {
                    FlightId = model.SelectedFlight,
                    NumberOfTickets = model.NumberOfSeats
                }
            );
        }

        if (!string.IsNullOrEmpty(model.PurchaseId))
        {
            return Redirect($"/purchase/{model.PurchaseId}");
        }

        return ViewComponent(typeof(FlightBookingFormViewComponent), model);
    }
}
