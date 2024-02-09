using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Common.Exceptions;
using TemporalAirlinesConcept.Common.Helpers;
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
    private readonly IFlightRepository _flightRepository;
    private readonly ITicketRepository _ticketRepository;

    public TicketService(ITemporalClient temporalClient, IFlightRepository flightRepository, ITicketRepository ticketRepository)
    {
        _temporalClient = temporalClient;
        _flightRepository = flightRepository;
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

    public async Task MarkAsPaid(string purchaseWorkflowId)
    {
        var handle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseWorkflowId);

        await handle.SignalAsync(x => x.SetAsPaid());
    }

    public async Task<string> RequestTicketPurchase(PurchaseModel purchaseModel)
    {
        await CreateFlightWorkflowIfNotExistsAsync(purchaseModel.FlightId);

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

    public Task SetPassengerDetails(string purchaseWorkflowId, List<string> passengerDetails)
    {
        var wh = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseWorkflowId);
        return wh.SignalAsync(wf => wf.SetPassengerDetails(passengerDetails));
    }

    private async Task CreateFlightWorkflowIfNotExistsAsync(string flightId)
    {
        if (await WorkflowHandleHelper.IsWorkflowRunning<FlightWorkflow>(_temporalClient, flightId))
            return;

        var flight = await _flightRepository.GetFlightAsync(flightId);

        if (flight is null)
            throw new EntityNotFoundException($"Flight {flightId} was not found.");

        await _temporalClient.StartWorkflowAsync((FlightWorkflow wf) => wf.Run(flight),
            new WorkflowOptions(flightId, Temporal.DefaultQueue));
    }

    public async Task<bool> RequestSeatReservation(SeatReservationInputModel seatReservationInputModel)
    {
        if (!await WorkflowHandleHelper.IsWorkflowRunning<FlightWorkflow>(_temporalClient,
                seatReservationInputModel.FlightId))
            return false;

        var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(seatReservationInputModel.FlightId);

        var flightDetails = await handle.QueryAsync(wf => wf.GetFlightDetails());

        var ticket = flightDetails.Registered.FirstOrDefault(t => t.Id == seatReservationInputModel.TicketId);

        if (ticket is null)
            return false;

        await handle.SignalAsync(wf => wf.ReserveSeat(
            new SeatReservationSignalModel(ticket, seatReservationInputModel.Seat.Name
        )));

        return true;
    }

    public async Task<bool> BoardPassenger(BoardingInputModel boardingInputModel)
    {
        if (!await WorkflowHandleHelper.IsWorkflowRunning<FlightWorkflow>(_temporalClient, boardingInputModel.FlightId))
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
