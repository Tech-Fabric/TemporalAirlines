using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TemporalAirlinesConcept.Common.Extensions;
using TemporalAirlinesConcept.Common.Settings;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Implementations.QRCodeGeneration;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;
using TemporalAirlinesConcept.Services.Models.QRCodeGeneration;
using Temporalio.Client;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase;

public class TicketService : ITicketService
{
    private readonly ITemporalClient _temporalClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UrlSettings _urlSettings;

    public TicketService(ITemporalClient temporalClient, IUnitOfWork unitOfWork, IOptions<UrlSettings> urlSettings)
    {
        _temporalClient = temporalClient;
        _unitOfWork = unitOfWork;
        _urlSettings = urlSettings.Value;
    }

    public async Task<Ticket> GetTicket(Guid ticketId)
    {
        var ticket = await _unitOfWork.Repository<Ticket>()
            .Get(x => x.Id == ticketId)
            .Include(x => x.Seat)
            .FirstOrDefaultAsync();

        return ticket;
    }

    public async Task<TicketWithCode> GetTicketWithCode(Guid ticketId)
    {
        var ticket = await _unitOfWork.Repository<Ticket>()
            .Get(x => x.Id == ticketId)
            .Include(x => x.Seat)
            .FirstOrDefaultAsync();

        var ticketWithCode = ticket == null ? null : GetTicketWithCode(ticket);

        return ticketWithCode;
    }

    public async Task<List<Ticket>> GetTickets(Guid userId)
    {
        var tickets = await _unitOfWork.Repository<Ticket>()
            .Get(x => x.UserId == userId)
            .ToListAsync();

        return tickets;
    }

    public async Task<List<Ticket>> GetTickets(Guid userId, Guid flightId)
    {
        var tickets = await _unitOfWork.Repository<Ticket>()
            .Get(x => x.UserId == userId && x.FlightId == flightId)
            .ToListAsync();

        return tickets;
    }

    public async Task<List<TicketWithCode>> GetPurchaseTickets(string purchaseId)
    {
        var ticketsWithCode = (await GetTickets(purchaseId))
            .Where(t => t.PurchaseId == purchaseId)
            .Select(GetTicketWithCode)
            .ToList();

        return ticketsWithCode;
    }

    public async Task<List<TicketWithCode>> GetPurchasePaidTickets(string purchaseId)
    {
        var ticketsWithCode = (await GetTickets(purchaseId))
            .Where(t => t.PurchaseId == purchaseId && t.PaymentStatus == DAL.Enums.PaymentStatus.Paid)
            .Select(GetTicketWithCode)
            .ToList();

        return ticketsWithCode;
    }

    private TicketWithCode GetTicketWithCode(Ticket ticket)
    {
        return new TicketWithCode
        {
            Id = ticket.Id,
            PurchaseId = ticket.PurchaseId,
            PaymentStatus = ticket.PaymentStatus,
            Seat = ticket.Seat?.Name,
            Passenger = ticket.Passenger,
            Code = QRCodeGeneratorService.Generate(new QRDataModel
            {
                Data = $"{_urlSettings.TicketPage}/{ticket.Id}"
            })
        };
    }

    private TicketWithCode GetTicketWithCode(TicketDetailsModel ticket)
    {
        return new TicketWithCode
        {
            Id = ticket.Id,
            PurchaseId = ticket.PurchaseId,
            PaymentStatus = ticket.PaymentStatus,
            Seat = ticket.Seat,
            Passenger = ticket.Passenger,
            Code = QRCodeGeneratorService.Generate(new QRDataModel
            {
                Data = $"{_urlSettings.TicketPage}/{ticket.Id}"
            })
        };
    }

    private async Task<List<TicketDetailsModel>> GetTickets(string purchaseId)
    {
        var purchaseHandle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseId);

        if (!await purchaseHandle.IsWorkflowRunningOrCompleted())
            return [];

        var flightId = await purchaseHandle.QueryAsync(wf => wf.GetFlightId());

        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(flightId))
            return [];

        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId);

        var tickets = await flightHandle.QueryAsync(wf => wf.GetRegisteredTickets());

        return tickets;
    }
}