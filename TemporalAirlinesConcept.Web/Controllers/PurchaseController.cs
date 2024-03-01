using Htmx;
using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Interfaces.Flight;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using TemporalAirlinesConcept.Web.ViewComponents.PurchaseForm;

namespace TemporalAirlinesConcept.Web.Controllers;

[Route("purchase")]
public class PurchaseController : Controller
{
    private readonly ITicketService _ticketService;
    private readonly IFlightService _flightService;
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(ITicketService ticketService, IFlightService flightService, IPurchaseService purchaseService)
    {
        _ticketService = ticketService;
        _flightService = flightService;
        _purchaseService = purchaseService;
    }

    [HttpGet("form")]
    public Task<IActionResult> Form()
    {
        if (Request.IsHtmx())
        {
            return Task.FromResult<IActionResult>(ViewComponent(typeof(PurchaseFormViewComponent)));
        }

        return Task.FromResult<IActionResult>(View("~/Views/Home/Index.cshtml"));
    }

    [HttpGet("{PurchaseId}")]
    public async Task<IActionResult> GetPurchaseDetails(
        [FromForm] PurchaseFormViewModel model,
        [FromRoute] string? purchaseId
    )
    {
        model.PurchaseId = purchaseId;

        model.IsPurchaseRunning = await _purchaseService.IsPurchaseRunning(purchaseId);

        if (!model.IsPurchaseRunning)
            return Error(model);

        model.Flight = await _flightService.GetFlightDetailsByPurchaseId(purchaseId);

        model.IsReservedAndPaid = await _purchaseService.IsReservedAndPaid(purchaseId);

        model.Tickets = await _ticketService.GetPurchasePaidTickets(purchaseId);

        if (Request.IsHtmx())
            return ViewComponent(typeof(PurchaseFormViewComponent), model);

        return View("~/Views/Purchase/Index.cshtml", model);
    }

    [HttpPost("{PurchaseId}/reserve-and-pay")]
    public async Task<IActionResult> ReserveAndPay(
        [FromForm] PurchaseFormViewModel model,
        [FromRoute] string? purchaseId
    )
    {
        model.PurchaseId = purchaseId;

        model.IsPurchaseRunning = await _purchaseService.IsPurchaseRunning(purchaseId);

        if (!model.IsPurchaseRunning)
            return Error(model);

        model.Flight = await _flightService.GetFlightDetailsByPurchaseId(purchaseId);
        model.NumberOfTickets = (await _ticketService.GetPurchaseTickets(purchaseId)).Count;

        var seatsList = model.SelectedSeats?
            .Where(s => s.Value)
            .Select(s => s.Key)
            .ToList();

        if (seatsList?.Count > model.NumberOfTickets)
        {
            ModelState.AddModelError(nameof(model.NumberOfTickets), "Too many seats selected");
        }

        if (ModelState.IsValid)
        {
            await _purchaseService.ReserveAndPaySeats(new SeatReservationInputModel
            {
                FlightId = model.Flight.Id,
                PurchaseId = model.PurchaseId,
                Seats = seatsList
            });

            model.IsReservedAndPaid = true;

            model.Tickets = await _ticketService.GetPurchasePaidTickets(model.PurchaseId);
        }

        if (Request.IsHtmx())
            return ViewComponent(typeof(PurchaseFormViewComponent), model);

        return View("~/Views/Purchase/Index.cshtml", model);
    }

    [HttpGet("{PurchaseId}/tickets")]
    public async Task<IActionResult> GetTickets(
        [FromRoute] string? purchaseId)
    {
        var model = new PurchaseFormViewModel();
        model.PurchaseId = purchaseId;

        model.IsPurchaseRunning = await _purchaseService.IsPurchaseRunning(purchaseId);

        if (!model.IsPurchaseRunning)
            return Error(model);

        model.IsReservedAndPaid = await _purchaseService.IsReservedAndPaid(purchaseId);

        model.Flight = await _flightService.GetFlightDetailsByPurchaseId(purchaseId);

        model.Tickets = await _ticketService.GetPurchasePaidTickets(purchaseId);

        if (Request.IsHtmx())
            return PartialView("Components/PurchaseForm/PurchaseTickets", model);

        return View("~/Views/Purchase/Index.cshtml", model);
    }

    private IActionResult Error(PurchaseFormViewModel model)
    {
        if (Request.IsHtmx())
            return PartialView("Components/PurchaseForm/Error");

        return View("~/Views/Purchase/Index.cshtml", model);
    }
}