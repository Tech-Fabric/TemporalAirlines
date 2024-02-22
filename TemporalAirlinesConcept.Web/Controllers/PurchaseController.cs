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
        
        model.Tickets = await _ticketService.GetPurchaseWorkflowTickets(purchaseId);

        model.Flight = await _flightService.GetFlightDetailsByPurchaseId(purchaseId);

        model.IsPaymentEmulated = await _ticketService.IsPurchasePaid(purchaseId);
        model.IsConfirmed = await _ticketService.IsSeatsReserved(purchaseId);
        
        if (Request.IsHtmx())
            return ViewComponent(typeof(PurchaseFormViewComponent), model);

        return View("~/Views/Purchase/Index.cshtml", model);
    }
    
    [HttpPost("{PurchaseId}/confirm")]
    public async Task<IActionResult> Details(
        [FromForm] PurchaseFormViewModel model,
        [FromRoute] string? purchaseId
    )
    {
        model.PurchaseId = purchaseId;

        model.Tickets = await _ticketService.GetPurchaseWorkflowTickets(purchaseId);
        
        model.Flight = await _flightService.GetFlightDetailsByPurchaseId(purchaseId);

        var seatsList = model.SelectedSeats?.Where(s => s.Value)
            .Select(s => s.Key).ToList();

        if (seatsList is not null && seatsList.Count > model.Tickets.Count)
        {
            ModelState.AddModelError(nameof(model.Tickets.Count), "Too many seats selected");
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
        
        model.IsPaymentEmulated = true;
        model.IsConfirmed = true;

        model.Tickets = await _ticketService.GetPurchaseWorkflowTickets(model.PurchaseId);

        model.Flight = await _flightService.GetFlightDetailsByPurchaseId(purchaseId);
        
        await _ticketService.MarkAsPaid(purchaseId);
        
        if (!Request.IsHtmx()) 
            return View("~/Views/Purchase/Index.cshtml", model);

        return ViewComponent(typeof(PurchaseFormViewComponent), model);
    }
}