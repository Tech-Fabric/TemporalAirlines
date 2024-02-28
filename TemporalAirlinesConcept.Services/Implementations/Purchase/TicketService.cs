using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TemporalAirlinesConcept.Common.Constants;
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
using Temporalio.Common;

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

    public async Task<bool> IsPurchaseRunning(string purchaseId)
    {
        var isPurchaseRunning = await _temporalClient.IsWorkflowRunning<PurchaseWorkflow>(purchaseId);

        return isPurchaseRunning;
    }

    public async Task<bool> IsPurchasePaid(string purchaseId)
    {
        if (!await _temporalClient.IsWorkflowRunning<PurchaseWorkflow>(purchaseId))
            throw new InvalidOperationException("Purchase workflow is not running.");

        var handle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseId);

        return await handle.QueryAsync(wf => wf.IsPaid());
    }

    public async Task<bool> IsSeatsReserved(string purchaseId)
    {
        if (!await _temporalClient.IsWorkflowRunning<PurchaseWorkflow>(purchaseId))
            throw new InvalidOperationException("Purchase workflow is not running.");

        var handle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseId);

        return await handle.QueryAsync(wf => wf.IsSeatsReserved());
    }

    public async Task MarkAsPaid(string purchaseId)
    {
        var handle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseId);

        await handle.SignalAsync(x => x.SetAsPaid());
    }

    public async Task<string> StartTicketPurchase(PurchaseModel purchaseModel)
    {
        var workflowId = Guid.NewGuid().ToString();

        await _temporalClient.StartWorkflowAsync<PurchaseWorkflow>(
            wf => wf.Run(purchaseModel), new WorkflowOptions
            {
                TaskQueue = Temporal.DefaultQueue,
                Id = workflowId,
                RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 1,
                    InitialInterval = TimeSpan.FromMinutes(5),
                    BackoffCoefficient = 2
                }
            });

        return workflowId;
    }

    public async Task<bool> RequestSeatReservation(SeatReservationInputModel seatReservationInputModel)
    {
        var purchaseId = seatReservationInputModel.PurchaseId;
        var flightId = seatReservationInputModel.FlightId.ToString();

        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(flightId)
            || !await _temporalClient.IsWorkflowRunning<PurchaseWorkflow>(purchaseId))
            return false;

        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId);
        var purchaseHandle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseId);

        var registered = await flightHandle.QueryAsync(wf => wf.GetRegisteredTickets());

        var tickets = registered
            .Where(t => t.PurchaseId == seatReservationInputModel.PurchaseId)
            .ToList();

        var seatReservations = tickets.Select((t, i) =>
                new SeatReservationSignalModel
                {
                    TicketId = t.Id,
                    Seat = seatReservationInputModel.Seats[i]
                })
            .ToList();

        var signalModel = new PurchaseTicketReservationSignal
        {
            SeatReservations = seatReservations,
            FlightId = seatReservationInputModel.FlightId.ToString()
        };

        await purchaseHandle.SignalAsync(x => x.TicketReservation(signalModel));

        return true;
    }

    public async Task<bool> BoardPassenger(BoardingInputModel boardingInputModel)
    {
        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(boardingInputModel.FlightId.ToString()))
            return false;

        var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(boardingInputModel.FlightId.ToString());

        var tickets = await handle.QueryAsync(wf => wf.GetRegisteredTickets());

        var ticket = tickets.FirstOrDefault(t => t.Id == boardingInputModel.TicketId);

        if (ticket is null)
            return false;

        await handle.SignalAsync(wf => wf.BoardPassenger(new BoardingSignalModel
        {
            Ticket = ticket
        }));

        return true;
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
        if (!await _temporalClient.IsWorkflowRunning<PurchaseWorkflow>(purchaseId))
            return [];

        var purchaseHandle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseId);

        var flightId = await purchaseHandle.QueryAsync(wf => wf.GetFlightId());

        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(flightId))
            return [];

        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId);

        var tickets = await flightHandle.QueryAsync(wf => wf.GetRegisteredTickets());

        return tickets;
    }

}