using Htmx;
using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using TemporalAirlinesConcept.Web.ViewComponents;

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
    public async Task<IActionResult> Flight([FromRoute] string? selectedFlight)
    {
        FlightBookingFormViewModel model = new FlightBookingFormViewModel();

        return await Flight(model, selectedFlight);
    }

    [HttpPost("{SelectedFlight}")]
    public async Task<IActionResult> Flight([FromForm] FlightBookingFormViewModel model, [FromRoute] string? selectedFlight)
    {
        if (!string.IsNullOrEmpty(selectedFlight))
        {
            model.SelectedFlight = selectedFlight;
        }

        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }
        else
        {
            return View("~/Views/Flight/Index.cshtml", model);
        }
    }

    [HttpPost("{SelectedFlight}/purchase")]
    public async Task<IActionResult> Purchase(
        [FromForm] FlightBookingFormViewModel model,
        [FromRoute] string? selectedFlight
    ) {
        if (!string.IsNullOrEmpty(selectedFlight))
        {
            model.SelectedFlight = selectedFlight;
        }

        var purchaseWorkflowId = "";

        if (!string.IsNullOrEmpty(model.CreditCardDetails.CardNumber))
        {
            purchaseWorkflowId = await _ticketService.RequestTicketPurchaseAsync(
                new PurchaseModel()
                {
                    FlightId = model.SelectedFlight
                }
            );

            // TODO: fetch workflow status

            model.PaymentSuccessful = true;
        }

        if (Request.IsHtmx())
        {
            if (!string.IsNullOrEmpty(purchaseWorkflowId))
            {
                Response.Htmx(h =>
                {
                    h.PushUrl($"/flights/{model.SelectedFlight}/ticket/{purchaseWorkflowId}");
                });
            }

            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }
        else
        {
            return View("~/Views/Flight/Index.cshtml", model);
        }
    }

    [HttpGet("{SelectedFlight}/ticket/{PurchaseWorkflowId}")]
    public async Task<IActionResult> Details(
        [FromRoute] string? selectedFlight,
        [FromRoute] string? purchaseWorkflowId
    )
    {
        FlightBookingFormViewModel model = new FlightBookingFormViewModel();

        if (!string.IsNullOrEmpty(selectedFlight))
        {
            model.SelectedFlight = selectedFlight;
        }


        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }
        else
        {
            return View("~/Views/Flight/Index.cshtml", model);
        }
    }

    [HttpPost("{SelectedFlight}/ticket/{PurchaseWorkflowId}")]
    public async Task<IActionResult> Details(
        [FromForm] FlightBookingFormViewModel model,
        [FromRoute] string? selectedFlight,
        [FromRoute] string? purchaseWorkflowId
    ) {
        if (!string.IsNullOrEmpty(selectedFlight))
        {
            model.SelectedFlight = selectedFlight;
        }

        var selectedSeatsCount = model.SelectedSeats.Count(kv => kv.Value == true);

        if (selectedSeatsCount > model.NumberOfSeats)
        {
            ModelState.AddModelError(nameof(model.NumberOfSeats), "Too many seats selected");
        }

        if (ModelState.IsValid)
        {

        }

        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }
        else
        {
            return View("~/Views/Flight/Index.cshtml", model);
        }
    }
}
