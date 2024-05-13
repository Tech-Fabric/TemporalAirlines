using Htmx;
using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using TemporalAirlinesConcept.Web.ViewComponents.FlightBookingForm;

namespace TemporalAirlinesConcept.Web.Controllers;

[Route("flights")]
public class FlightController : Controller
{
    private readonly IPurchaseService _purchaseService;

    public FlightController(IPurchaseService purchaseService)
    {
        _purchaseService = purchaseService;
    }

    [HttpGet]
    public async Task<IActionResult> Form()
    {
        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent));
        }

        return View("~/Views/Flight/Index.cshtml");
    }

    [HttpPost]
    public IActionResult Form([FromForm] FlightBookingFormViewModel model)
    {
        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }

        return View("~/Views/Flight/Index.cshtml", model);
    }

    [HttpGet("{SelectedFlight}")]
    public async Task<IActionResult> Flight([FromRoute] Guid? selectedFlight)
    {
        FlightBookingFormViewModel model = new();

        return await Flight(model, selectedFlight);
    }

    [HttpPost("{SelectedFlight}")]
    public async Task<IActionResult> Flight([FromForm] FlightBookingFormViewModel model,
        [FromRoute] Guid? selectedFlight)
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
            model.PurchaseId = await _purchaseService.StartPurchase(
                new PurchaseModel
                {
                    FlightId = model.SelectedFlight.Value,
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