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

        [HttpPost("purchase")]
        public async Task PurchaseTicketAsync(PurchaseInputModel purchaseInputModel)
        {
            await _ticketService.RequestTicketPurchaseAsync(purchaseInputModel);
        }
    }
}
