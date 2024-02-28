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

    public PurchaseController(ITicketService ticketService, IFlightService flightService)
    {
        _ticketService = ticketService;
        _flightService = flightService;
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

        model.IsPurchaseRunning = await _ticketService.IsPurchaseRunning(purchaseId);

        if (!model.IsPurchaseRunning)
            return Error(model);

        model.Flight = await _flightService.GetFlightDetailsByPurchaseId(purchaseId);

        model.IsPaymentEmulated = await _ticketService.IsPurchasePaid(purchaseId);
        model.IsConfirmed = await _ticketService.IsSeatsReserved(purchaseId);

        model.Tickets = await _ticketService.GetPurchasePaidTickets(purchaseId);

        if (Request.IsHtmx())
            return ViewComponent(typeof(PurchaseFormViewComponent), model);

        return View("~/Views/Purchase/Index.cshtml", model);
    }

    [HttpPost("{PurchaseId}/confirm")]
    public async Task<IActionResult> Confirm(
        [FromForm] PurchaseFormViewModel model,
        [FromRoute] string? purchaseId
    )
    {
        model.PurchaseId = purchaseId;

        model.IsPurchaseRunning = await _ticketService.IsPurchaseRunning(purchaseId);

        if (!model.IsPurchaseRunning)
            return Error(model);

        model.Flight = await _flightService.GetFlightDetailsByPurchaseId(purchaseId);

        var tickets = await _ticketService.GetPurchaseTickets(purchaseId);

        model.NumberOfTickets = tickets.Count;

        var seatsList = model.SelectedSeats?.Where(s => s.Value)
            .Select(s => s.Key).ToList();

        if (seatsList is not null && seatsList.Count > tickets.Count)
        {
            ModelState.AddModelError(nameof(model.NumberOfTickets), "Too many seats selected");
        }

        if (ModelState.IsValid)
        {
            await _ticketService.RequestSeatReservation(new SeatReservationInputModel
            {
                FlightId = model.Flight.Id,
                PurchaseId = model.PurchaseId,
                Seats = seatsList
            });

            model.IsConfirmed = true;
        }

        if (Request.IsHtmx())
            return ViewComponent(typeof(PurchaseFormViewComponent), model);

        return View("~/Views/Purchase/Index.cshtml", model);
    }

    [HttpPost("{PurchaseId}/payment")]
    public async Task<IActionResult> MarkAsPaid(
        [FromForm] PurchaseFormViewModel model,
        [FromRoute] string? purchaseId)
    {
        model.PurchaseId = purchaseId;

        model.IsPurchaseRunning = await _ticketService.IsPurchaseRunning(purchaseId);

        if (!model.IsPurchaseRunning)
            return Error(model);

        model.IsPaymentEmulated = true;
        model.IsConfirmed = true;

        model.Tickets = await _ticketService.GetPurchasePaidTickets(model.PurchaseId);

        model.Flight = await _flightService.GetFlightDetailsByPurchaseId(purchaseId);

        await _ticketService.MarkAsPaid(purchaseId);

        if (!Request.IsHtmx())
            return View("~/Views/Purchase/Index.cshtml", model);

        return ViewComponent(typeof(PurchaseFormViewComponent), model);
    }

    [HttpGet("{PurchaseId}/tickets")]
    public async Task<IActionResult> GetTickets(
        [FromRoute] string? purchaseId)
    {
        var model = new PurchaseFormViewModel();
        model.PurchaseId = purchaseId;

        model.IsPurchaseRunning = await _ticketService.IsPurchaseRunning(purchaseId);

        if (!model.IsPurchaseRunning)
            return Error(model);

        model.IsPaymentEmulated = await _ticketService.IsPurchasePaid(purchaseId);
        model.IsConfirmed = await _ticketService.IsSeatsReserved(purchaseId);

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