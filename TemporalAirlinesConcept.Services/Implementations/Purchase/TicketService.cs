using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Common.Extensions;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Client;
using Temporalio.Common;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase;

public class TicketService : ITicketService
{
    private readonly ITemporalClient _temporalClient;
    private readonly ITicketRepository _ticketRepository;

    public TicketService(ITemporalClient temporalClient, ITicketRepository ticketRepository)
    {
        _temporalClient = temporalClient;
        _ticketRepository = ticketRepository;
    }

    public async Task<Ticket> GetTicket(string ticketId)
    {
        var ticket = await _ticketRepository.GetTicketAsync(ticketId);

        return ticket;
    }

    public async Task<List<Ticket>> GetTickets(string userId)
    {
        var tickets = await _ticketRepository.GetTicketsByUserIdAsync(userId);

        return tickets;
    }

    public async Task<List<Ticket>> GetTickets(string userId, string flightId)
    {
        var tickets = await _ticketRepository.GetTicketsByUserIdFlightAsync(userId, flightId);

        return tickets;
    }

    public async Task<List<Ticket>> GetPurchaseWorkflowTickets(string purchaseWorkflowId)
    {
        if (!await _temporalClient.IsWorkflowRunning<PurchaseWorkflow>(purchaseWorkflowId))
            return [];

        var handle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseWorkflowId);

        var tickets = await handle.QueryAsync(wf => wf.GetTickets());

        return tickets;
    }

    public async Task MarkAsPaid(string purchaseWorkflowId)
    {
        var handle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseWorkflowId);

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
        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(seatReservationInputModel.FlightId)
            || !await _temporalClient.IsWorkflowRunning<PurchaseWorkflow>(seatReservationInputModel.PurchaseId))
            return false;

        var purchaseHandle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(seatReservationInputModel.PurchaseId);
        var tickets = await purchaseHandle.QueryAsync(wf => wf.GetTickets());

        var seatReservations = tickets.Select((t, i) =>
            new SeatReservationSignalModel { TicketId = t.Id, Seat = seatReservationInputModel.Seats[i] }).ToList();

        var signalModel = new PurchaseTicketReservationSignal
        {
            SeatReservations = seatReservations,
            FlightId = seatReservationInputModel.FlightId
        };

        await purchaseHandle.SignalAsync(x => x.TicketReservation(signalModel));

        return true;
    }

    public async Task<bool> BoardPassenger(BoardingInputModel boardingInputModel)
    {
        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(boardingInputModel.FlightId))
            return false;

        var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(boardingInputModel.FlightId);

        var flightDetails = await handle.QueryAsync(wf => wf.GetFlightDetails());

        var ticket = flightDetails.Registered.FirstOrDefault(t => t.Id == boardingInputModel.TicketId);

        if (ticket is null)
            return false;

        await handle.SignalAsync(wf => wf.BoardPassenger(new BoardingSignalModel
        {
            Ticket = ticket
        }));

        return true;
    }
}
