using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Api.Controllers;

[Route("api/tickets")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }
    
    [HttpPost("purchase")]
    public async Task<ActionResult<string>> PurchaseTicket(PurchaseModel purchaseModel)
    {
        return Ok(await _ticketService.RequestTicketPurchase(purchaseModel));
    }

    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<string>> GetTickets([FromRoute] string userId)
    {
        var tickets = await _ticketService.GetTickets(userId);

        return Ok(tickets);
    }

    [HttpGet("{userId}/{flightId}")]
    public async Task<ActionResult<string>> GetTicket([FromRoute] string userId, [FromRoute] string flightId)
    {
        var tickets = await _ticketService.GetTickets(userId, flightId);

        return Ok(tickets);
    }

    [HttpGet("{ticketId}")]
    public async Task<ActionResult<Ticket>> GetTicket([FromRoute] string ticketId)
    {
        var ticket = await _ticketService.GetTicket(ticketId);

        return Ok(ticket);
    }

    [HttpPatch("{purchaseWorkflowId}")]
    public async Task<ActionResult<Ticket>> MarkAsPaid([FromRoute] string purchaseWorkflowId)
    {
        await _ticketService.MarkAsPaid(purchaseWorkflowId);

        return Ok();
    }
    
    [HttpPost("check-in")]
    public async Task<ActionResult<bool>> ReserveSeat(SeatReservationInputModel seatReservationInputModel)
    {
        return Ok(await _ticketService.RequestSeatReservation(seatReservationInputModel));
    }
    
    [HttpPost("board")]
    public async Task<ActionResult<bool>> BoardPassenger(BoardingInputModel boardingInputModel)
    {
        return Ok(await _ticketService.BoardPassenger(boardingInputModel));
    }
}
