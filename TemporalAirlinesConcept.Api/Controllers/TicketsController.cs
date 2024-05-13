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
    private readonly IPurchaseService _purchaseService;
    private readonly IMapper _mapper;

    public TicketsController(ITicketService ticketService, IMapper mapper, IPurchaseService purchaseService)
    {
        _ticketService = ticketService;
        _mapper = mapper;
        _purchaseService = purchaseService;
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<IdResponse>> StartTicketPurchase(StartPurchaseRequest startPurchaseRequest)
    {
        var purchaseModel = _mapper.Map<PurchaseModel>(startPurchaseRequest);
        var workflowId = await _purchaseService.StartPurchase(purchaseModel);

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

    [HttpPatch("{purchaseId}")]
    public async Task<IActionResult> MarkAsPaid([FromRoute] string purchaseId)
    {
        await _purchaseService.MarkAsPaid(purchaseId);

        return Ok();
    }

    [HttpPost("check-in")]
    public async Task<IActionResult> ReserveSeat(SeatReservationInputModel seatReservationInputModel)
    {
        await _purchaseService.RequestSeatReservation(seatReservationInputModel);

        return Ok();
    }

    [HttpPost("board")]
    public async Task<ActionResult<ResultStatusResponse>> BoardPassenger(BoardingInputModel boardingInputModel)
    {
        var boardResult = await _purchaseService.BoardPassenger(boardingInputModel);

        var result = new ResultStatusResponse
        {
            Result = boardResult
        };

        return Ok(result);
    }
}