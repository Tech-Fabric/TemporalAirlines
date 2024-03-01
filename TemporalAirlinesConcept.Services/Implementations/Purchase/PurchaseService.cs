using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Common.Extensions;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Client;
using Temporalio.Common;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase;

public class PurchaseService : IPurchaseService
{
    private readonly ITemporalClient _temporalClient;

    public PurchaseService(ITemporalClient temporalClient)
    {
        _temporalClient = temporalClient;
    }

    public async Task<string> StartPurchase(PurchaseModel purchaseModel)
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

    public async Task RequestSeatReservation(SeatReservationInputModel seatReservationInputModel)
    {
        var purchaseId = seatReservationInputModel.PurchaseId;
        var flightId = seatReservationInputModel.FlightId.ToString();

        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId);
        var purchaseHandle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseId);

        await ReserveSeats(seatReservationInputModel, flightHandle, purchaseHandle);
    }

    public async Task ReserveAndPaySeats(SeatReservationInputModel seatReservationInputModel)
    {
        var purchaseId = seatReservationInputModel.PurchaseId;
        var flightId = seatReservationInputModel.FlightId.ToString();

        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId);
        var purchaseHandle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseId);

        await ReserveSeats(seatReservationInputModel, flightHandle, purchaseHandle);

        await purchaseHandle.SignalAsync(x => x.SetAsPaid());
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

    public async Task<bool> IsPurchaseRunning(string purchaseId)
    {
        var isPurchaseRunning = await _temporalClient.IsWorkflowRunning<PurchaseWorkflow>(purchaseId);

        return isPurchaseRunning;
    }

    public async Task<bool> IsPaid(string purchaseId)
    {
        var handle = await GetPurchaseWorkflow(purchaseId);

        return await handle.QueryAsync(wf => wf.IsPaid());
    }

    public async Task<bool> IsSeatsReserved(string purchaseId)
    {
        var handle = await GetPurchaseWorkflow(purchaseId);

        return await handle.QueryAsync(wf => wf.IsSeatsReserved());
    }

    public async Task<bool> IsReservedAndPaid(string purchaseId)
    {
        var handle = await GetPurchaseWorkflow(purchaseId);

        var isReservedAndPaid = await handle.QueryAsync(wf => wf.IsPaid()) &&
                                await handle.QueryAsync(wf => wf.IsSeatsReserved());

        return isReservedAndPaid;
    }

    public async Task MarkAsPaid(string purchaseId)
    {
        var handle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseId);

        await handle.SignalAsync(x => x.SetAsPaid());
    }

    private static async Task ReserveSeats(SeatReservationInputModel seatReservationInputModel,
        WorkflowHandle<FlightWorkflow> flightHandle,
        WorkflowHandle<PurchaseWorkflow> purchaseHandle)
    {
        if (!await flightHandle.IsWorkflowRunning() || !await purchaseHandle.IsWorkflowRunning())
            return;

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
    }

    private async Task<WorkflowHandle<PurchaseWorkflow>> GetPurchaseWorkflow(string purchaseId)
    {
        var handle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseId);

        if (!await _temporalClient.IsWorkflowRunning<PurchaseWorkflow>(purchaseId))
            throw new InvalidOperationException("Purchase workflow is not running.");

        return handle;
    }
}