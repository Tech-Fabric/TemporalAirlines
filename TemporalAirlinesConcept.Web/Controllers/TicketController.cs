using Htmx;
using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Web.ViewComponents.TicketDetails;

namespace TemporalAirlinesConcept.Web.Controllers;

[Route("ticket")]
public class TicketController : Controller
{
    private readonly ITicketService _ticketService;

    public TicketController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("{TicketId}")]
    public async Task<IActionResult> GetTicketDetails(
        [FromForm] TicketDetailsViewModel model,
        [FromRoute] string? ticketId
    )
    {
        model.Ticket = await _ticketService.GetTicket(ticketId);
        
        if (Request.IsHtmx())
        {
            return ViewComponent(typeof(TicketDetailsViewComponent), model);
        }

        return View("~/Views/Ticket/Index.cshtml", model);
    }
}