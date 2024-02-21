using Htmx;
using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Web.ViewComponents.PurchaseDetails;
using TemporalAirlinesConcept.Web.ViewComponents.FlightBookingForm;

namespace TemporalAirlinesConcept.Web.Controllers;

[Route("purchase")]
public class PurchaseController : Controller
{
    private readonly ITicketService _ticketService;

    public PurchaseController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("{PurchaseWorkflowId}")]
    public async Task<IActionResult> GetPurchaseDetails(
        [FromForm] PurchaseDetailsViewModel model,
        [FromRoute] string? purchaseWorkflowId
    )
    {
        model.Tickets = await _ticketService.GetPurchaseWorkflowTickets(purchaseWorkflowId);
        
        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(FlightBookingFormViewComponent), model);
        }

        return View("~/Views/Purchase/Index.cshtml", model);
    }
}