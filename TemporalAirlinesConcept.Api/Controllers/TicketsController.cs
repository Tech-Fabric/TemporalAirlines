using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Api.Controllers
{
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
    }
}
