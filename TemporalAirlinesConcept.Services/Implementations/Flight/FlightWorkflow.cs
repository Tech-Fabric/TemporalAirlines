using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.Services.Models.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Exceptions;
using Temporalio.Workflows;

namespace TemporalAirlinesConcept.Services.Implementations.Flight;

[Workflow]
public class FlightWorkflow
{
    private readonly ActivityOptions _activityOptions = new()
    {
        StartToCloseTimeout = TimeSpan.FromSeconds(30)
    };

    private FlightDetailsModel _flight;

    [WorkflowRun]
    public async Task Run(DAL.Entities.Flight flight)
    {
        _flight = await Workflow.ExecuteActivityAsync((FlightActivities act) => act.MapFlightModel(flight),
            _activityOptions);

        await ChangeStatusAtTime(FlightStatus.CheckIn, _flight.Depart.Value.Subtract(TimeSpan.FromDays(1)));

        _flight = await Workflow.ExecuteActivityAsync((FlightActivities act) =>
            act.AssignSeats(_flight), _activityOptions);

        await ChangeStatusAtTime(FlightStatus.Boarding, _flight.Depart.Value.Subtract(TimeSpan.FromHours(2)));

        await ChangeStatusAtTime(FlightStatus.Closed, _flight.Depart.Value.Subtract(TimeSpan.FromMinutes(5)));

        await ChangeStatusAtTime(FlightStatus.Departed, _flight.Depart.Value);

        await ChangeStatusAtTime(FlightStatus.Arrived, _flight.Arrival.Value);

        await Workflow.ExecuteActivityAsync((FlightActivities act) => act.SaveFlightDetails(_flight),
            _activityOptions);
    }

    private async Task ChangeStatusAtTime(FlightStatus status, DateTime time)
    {
        var delay = time.Subtract(Workflow.UtcNow);

        if (delay > TimeSpan.Zero)
        {
            await Workflow.DelayAsync(delay);
        }

        _flight.Status = status;
    }

    /// <summary>
    /// Registers a ticket for booking.
    /// </summary>
    /// <param name="bookingSignalModel">The booking request model.</param>
    [WorkflowSignal]
    public Task Book(BookingSignalModel bookingSignalModel)
    {
        _flight.Registered.Add(bookingSignalModel.Ticket);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes a ticket booking.
    /// </summary>
    /// <param name="bookingSignalModel">The model containing the booking request information.</param>
    [WorkflowSignal]
    public Task BookCompensation(BookingSignalModel bookingSignalModel)
    {
        var bookedTicket = _flight.Registered.FirstOrDefault(s => s.Id == bookingSignalModel.Ticket.Id);

        if (bookedTicket is not null)
            _flight.Registered.Remove(bookedTicket);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks a ticket as paid.
    /// </summary>
    /// <param name="markTicketPaidSignalModel">The request model containing the ticket to be marked as paid.</param>
    [WorkflowSignal]
    public Task MarkTicketPaid(MarkTicketPaidSignalModel markTicketPaidSignalModel)
    {
        foreach (var ticket in _flight.Registered.Where(ticket => 
                     ticket.PurchaseId == markTicketPaidSignalModel.PurchaseId))
        {
            ticket.PaymentStatus = PaymentStatus.Paid;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks a ticket as cancelled.
    /// </summary>
    /// <param name="markTicketPaidSignalModel">The model containing information about the ticket to mark as paid.</param>
    [WorkflowSignal]
    public Task MarkTicketPaidCompensation(MarkTicketPaidSignalModel markTicketPaidSignalModel)
    {
        foreach (var ticket in _flight.Registered.Where(ticket => 
                     ticket.PurchaseId == markTicketPaidSignalModel.PurchaseId))
        {
            ticket.PaymentStatus = PaymentStatus.Cancelled;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Reserves a seat for a ticket in the flight.
    /// </summary>
    /// <param name="seatReservationSignalModel">The model containing the seat and ticket information.</param>
    [WorkflowSignal]
    public Task ReserveSeat(SeatReservationSignalModel seatReservationSignalModel)
    {
        var seat = _flight.Seats.FirstOrDefault(f => f.Name == seatReservationSignalModel.Seat);

        if (seat is null)
            throw new ApplicationFailureException($"Seat {seatReservationSignalModel.Seat} was not found.");

        seat.TicketId = seatReservationSignalModel.TicketId;

        var ticket = _flight.Registered.FirstOrDefault(t => t.Id == seatReservationSignalModel.TicketId);

        if (ticket is null)
            throw new ApplicationFailureException($"Ticket {seatReservationSignalModel.TicketId} was not found.");

        ticket.Seat = seat;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes seat reservation.
    /// </summary>
    /// <param name="seatReservationSignalModel">The seat reservation request model that contains the seat information.</param>
    [WorkflowSignal]
    public Task ReserveSeatCompensation(SeatReservationSignalModel seatReservationSignalModel)
    {
        var registeredTicket = _flight.Registered.FirstOrDefault(t => t.Id == seatReservationSignalModel.TicketId);

        if (registeredTicket is null)
            return Task.CompletedTask;

        registeredTicket.Seat = null;

        var seat = _flight.Seats.FirstOrDefault(f => f.Name == seatReservationSignalModel.Seat);

        if (seat is null)
            return Task.CompletedTask;

        seat.TicketId = null;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds a passenger's ticket to the boarded list of a flight.
    /// </summary>
    /// <param name="boardingSignalModel">The model containing the information of the passenger's boarding request.</param>
    [WorkflowSignal]
    public Task BoardPassenger(BoardingSignalModel boardingSignalModel)
    {
        _flight.Boarded.Add(boardingSignalModel.Ticket);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves the flight details.
    /// </summary>
    /// <returns>The flight details.</returns>
    [WorkflowQuery]
    public FlightDetailsModel GetFlightDetails()
    {
        return _flight;
    }
    
    [WorkflowQuery]
    public List<Ticket> GetRegisteredTickets()
    {
        return _flight.Registered;
    }
}
