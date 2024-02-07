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

        // POST: api/tickets/purchase
        [HttpPost("purchase")]
        public async Task<ActionResult<string>> PurchaseTicketAsync(PurchaseModel purchaseModel)
        {
            return Ok(await _ticketService.RequestTicketPurchaseAsync(purchaseModel));
        }
        
        // POST: api/tickets/check-in
        [HttpPost("check-in")]
        public async Task<ActionResult<bool>> ReserveSeatAsync(SeatReservationInputModel seatReservationInputModel)
        {
            return Ok(await _ticketService.RequestSeatReservationAsync(seatReservationInputModel));
        }

        // POST: api/tickets/board
        [HttpPost("board")]
        public async Task<ActionResult<bool>> BoardPassengerAsync(BoardingInputModel boardingInputModel)
        {
            return Ok(await _ticketService.BoardPassengerAsync(boardingInputModel));
        }

    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<string>> GetTicketsAsync([FromRoute] string userId)
    {
        var tickets = await _ticketService.GetTickets(userId);

        return Ok(tickets);
    }

    [HttpGet("{userId}/{flightId}")]
    public async Task<ActionResult<string>> GetTicketAsync([FromRoute] string userId, [FromRoute] string flightId)
    {
        var tickets = await _ticketService.GetTickets(userId, flightId);

        return Ok(tickets);
    }

    [HttpGet("{ticketId}")]
    public async Task<ActionResult<Ticket>> GetTicketAsync([FromRoute] string ticketId)
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
}
