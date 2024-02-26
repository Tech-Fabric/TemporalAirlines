using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Api.Models.Base;
using TemporalAirlinesConcept.Api.Models.Tickets;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Api.Controllers;

[Route("api/tickets")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly IMapper _mapper;

    public TicketsController(ITicketService ticketService, IMapper mapper)
    {
        _ticketService = ticketService;
        _mapper = mapper;
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<IdResponse>> StartTicketPurchase(StartPurchaseRequest startPurchaseRequest)
    {
        var purchaseModel = _mapper.Map<PurchaseModel>(startPurchaseRequest);
        var workflowId = await _ticketService.StartTicketPurchase(purchaseModel);

        var response = new IdResponse
        {
            Id = workflowId
        };

        return Ok(response);
    }

    [HttpGet("by-user/{userId}")]
    public async Task<ActionResult<List<TicketResponse>>> GetTickets([FromRoute] Guid userId)
    {
        var tickets = await _ticketService.GetTickets(userId);
        var ticketsResponse = _mapper.Map<List<TicketResponse>>(tickets);

        return Ok(ticketsResponse);
    }

    [HttpGet("{userId}/{flightId}")]
    public async Task<ActionResult<List<TicketResponse>>> GetTicket([FromRoute] Guid userId, [FromRoute] Guid flightId)
    {
        var tickets = await _ticketService.GetTickets(userId, flightId);
        var ticketsResponse = _mapper.Map<List<TicketResponse>>(tickets);

        return Ok(ticketsResponse);
    }

    [HttpGet("{ticketId}")]
    public async Task<ActionResult<TicketResponse>> GetTicket([FromRoute] Guid ticketId)
    {
        var ticket = await _ticketService.GetTicket(ticketId);
        var ticketResponse = _mapper.Map<TicketResponse>(ticket);

        return Ok(ticketResponse);
    }

    [HttpPatch("{purchaseWorkflowId}")]
    public async Task<IActionResult> MarkAsPaid([FromRoute] string purchaseWorkflowId)
    {
        await _ticketService.MarkAsPaid(purchaseWorkflowId);

        return Ok();
    }

    [HttpPost("check-in")]
    public async Task<ActionResult<ResultStatusResponse>> ReserveSeat(SeatReservationInputModel seatReservationInputModel)
    {
        var reservationResult = await _ticketService.RequestSeatReservation(seatReservationInputModel);

        var result = new ResultStatusResponse
        {
            Result = reservationResult
        };

        return Ok(result);
    }

    [HttpPost("board")]
    public async Task<ActionResult<ResultStatusResponse>> BoardPassenger(BoardingInputModel boardingInputModel)
    {
        var boardResult = await _ticketService.BoardPassenger(boardingInputModel);

        var result = new ResultStatusResponse
        {
            Result = boardResult
        };

        return Ok(result);
    }
}
