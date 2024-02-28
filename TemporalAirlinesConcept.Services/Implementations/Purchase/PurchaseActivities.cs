using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Common.Extensions;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Models.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Activities;
using Temporalio.Client;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase;

public class PurchaseActivities
{
    private readonly ITemporalClient _temporalClient;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseActivities(ITemporalClient temporalClient, IUnitOfWork unitOfWork)
    {
        _temporalClient = temporalClient;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Checks whether the flight specified by the flight IDs is available for booking.
    /// </summary>
    /// <param name="flightAvailabilityModel"></param>
    /// <returns>Returns true if flight is available; otherwise, false.</returns>
    [Activity]
    public async Task<bool> IsFlightAvailable(FlightAvailabilityModel flightAvailabilityModel)
    {
        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightAvailabilityModel.FlightId.ToString());

        var flight = await flightHandle.QueryAsync(wf => wf.GetFlightDetails());

        var isAnySeatsLeft = (flight.Seats.Count - flight.Registered.Count) >= flightAvailabilityModel.NumberOfTickets;

        return isAnySeatsLeft;
    }

    /// <summary>
    /// Creates a ticket and books the corresponding flight.
    /// </summary>
    /// <param name="ticket">The ticket object to be created.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result indicates whether
    /// the ticket creation was successful or not.</returns>
    [Activity]
    public async Task<bool> BookTicket(TicketDetailsModel ticket)
    {
        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(ticket.FlightId.ToString());

        await flightHandle.SignalAsync(wf => wf.Book(new BookingSignalModel
        {
            Ticket = ticket
        }));

        return true;
    }

    /// <summary>
    /// Removes a ticket from the registered flight.
    /// </summary>
    /// <param name="ticket">The ticket object representing the ticket which needs to be removed.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating
    /// whether the ticket was removed successfully or not.</returns>
    [Activity]
    public async Task<bool> BookTicketCompensation(TicketDetailsModel ticket)
    {
        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(ticket.FlightId.ToString());

        await flightHandle.SignalAsync(wf => wf.BookCompensation(new BookingSignalModel
        {
            Ticket = ticket
        }));

        return true;
    }

    [Activity]
    public Task<bool> HoldMoney()
    {
        return Task.FromResult(true);
    }

    [Activity]
    public Task<bool> HoldMoneyCompensation()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Marks a ticket as paid.
    /// </summary>
    /// <returns>A boolean value indicating whether the ticket was marked as paid successfully.</returns>
    [Activity]
    public async Task<bool> MarkTicketAsPaid(MarkTicketPaidSignalModel markTicketPaidSignalModel)
    {
        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(markTicketPaidSignalModel.FlightId);

        await flightHandle.SignalAsync(wf => wf.MarkTicketPaid(markTicketPaidSignalModel));

        return true;
    }

    /// <summary>
    /// Marks a ticket as canceled.
    /// </summary>
    /// <returns>A boolean value indicating whether the tickets was marked as canceled successfully.</returns>
    [Activity]
    public async Task<bool> MarkTicketAsPaidCompensation(MarkTicketPaidSignalModel markTicketPaidSignalModel)
    {
        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(markTicketPaidSignalModel.FlightId);

        await flightHandle.SignalAsync(wf => wf.MarkTicketPaidCompensation(markTicketPaidSignalModel));

        return true;
    }

    /// <summary>
    /// Saves tickets to blob.
    /// </summary>
    /// <returns>A boolean value indicating whether the tickets was generated successfully.</returns>
    [Activity]
    public Task<List<TicketBlobModel>> GenerateBlobTickets()
    {
        return Task.FromResult(new List<TicketBlobModel>());
    }

    /// <summary>
    /// Deletes tickets from blob.
    /// </summary>
    /// <returns>A boolean value indicating whether the tickets was removed successfully.</returns>
    [Activity]
    public Task<bool> GenerateBlobTicketsCompensation()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Sends tickets.
    /// </summary>
    /// <returns>A boolean value indicating whether the tickets was sent successfully.</returns>
    [Activity]
    public Task<bool> SendTickets()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Sends tickets compensation.
    /// </summary>
    /// <returns>A boolean value indicating whether the tickets compensation was sent successfully.</returns>
    [Activity]
    public Task<bool> SendTicketsCompensation()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Saves a list of tickets.
    /// </summary>
    /// <param name="saveTicketsSignal"></param>
    /// <returns>A boolean indicating whether the tickets were saved successfully.</returns>
    [Activity]
    public async Task<bool> SaveTickets(SaveTicketsSignalModel saveTicketsSignal)
    {
        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(saveTicketsSignal.FlightId);

        var saveSignal = new SavePurchaseTicketsSignalModel
        {
            PurchaseId = saveTicketsSignal.PurchaseId
        };

        await flightHandle.SignalAsync(wf => wf.SaveTickets(saveSignal));

        return true;
    }

    /// <summary>
    /// Removes the tickets.
    /// </summary>
    /// <param name="saveTicketsSignal"></param>
    /// <returns>The task result is true if the operation is successful; otherwise, false.</returns>
    [Activity]
    public async Task<bool> SaveTicketsCompensation(SaveTicketsSignalModel saveTicketsSignal)
    {
        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(saveTicketsSignal.FlightId);

        var saveSignal = new SavePurchaseTicketsSignalModel
        {
            PurchaseId = saveTicketsSignal.PurchaseId
        };

        await flightHandle.SignalAsync(wf => wf.SaveTicketsCompensation(saveSignal));

        return true;
    }

    /// <summary>
    /// Confirms a withdrawal.
    /// </summary>
    [Activity]
    public Task<bool> ConfirmWithdraw()
    {
        return Task.FromResult(true);
    }

    [Activity]
    public Task<bool> ConfirmWithdrawCompensation()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Retrieves the last flight from the given list of flight IDs.
    /// </summary>
    /// <param name="flightId"></param>
    [Activity]
    public async Task<DAL.Entities.Flight> GetFlight(Guid flightId)
    {
        var flight = await _unitOfWork.Repository<DAL.Entities.Flight>()
            .FindAsync(x => x.Id == flightId);

        if (string.Equals(flight?.From, Airports.ErrorCode) || string.Equals(flight?.To, Airports.ErrorCode))
            throw new Exception("Artificial error exception");

        return flight;
    }

    [Activity]
    public async Task<FlightDetailsModel> GetFlightDetails(Guid flightId)
    {
        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(flightId.ToString()))
            throw new ApplicationException("Flight workflow is not running.");

        var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId.ToString());

        return await handle.QueryAsync(wf => wf.GetFlightDetails());
    }

    [Activity]
    public async Task TicketReservation(PurchaseTicketReservationSignal purchaseTicketReservation)
    {
        if (purchaseTicketReservation?.SeatReservations is null)
            return;

        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(purchaseTicketReservation.FlightId);

        foreach (var seatReservation in purchaseTicketReservation?.SeatReservations)
        {
            await flightHandle.SignalAsync(wf => wf.ReserveSeat(seatReservation));
        }
    }

    [Activity]
    public async Task TicketReservationCompensation(PurchaseTicketReservationSignal purchaseTicketReservation)
    {
        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(purchaseTicketReservation.FlightId);

        if (purchaseTicketReservation.SeatReservations is null)
            return;

        foreach (var seatReservation in purchaseTicketReservation?.SeatReservations)
        {
            await flightHandle.SignalAsync(wf => wf.ReserveSeatCompensation(seatReservation));
        }
    }
}