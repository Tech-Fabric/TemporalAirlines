using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.Services.Models.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;
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
    public async Task RunAsync(DAL.Entities.Flight flight)
    {
        _flight = await Workflow.ExecuteActivityAsync((FlightActivities act) => act.MapFlightModelAsync(flight),
            _activityOptions);

        await ChangeStatusAtTimeAsync(FlightStatus.CheckIn, _flight.Depart.Subtract(TimeSpan.FromDays(1)));

        _flight = await Workflow.ExecuteActivityAsync((FlightActivities act) =>
            act.AssignSeatsAsync(_flight), _activityOptions);

        await ChangeStatusAtTimeAsync(FlightStatus.Boarding, _flight.Depart.Subtract(TimeSpan.FromHours(2)));

        await ChangeStatusAtTimeAsync(FlightStatus.Closed, _flight.Depart.Subtract(TimeSpan.FromMinutes(5)));

        await ChangeStatusAtTimeAsync(FlightStatus.Departed, _flight.Depart);

        await ChangeStatusAtTimeAsync(FlightStatus.Arrived, _flight.Arrival);

        await Workflow.ExecuteActivityAsync((FlightActivities act) => act.SaveFlightDetailsAsync(_flight),
            _activityOptions);
    }

    private async Task ChangeStatusAtTimeAsync(FlightStatus status, DateTime time)
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
    public async Task BookAsync(BookingSignalModel bookingSignalModel)
    {
        _flight.Registered.Add(bookingSignalModel.Ticket);
    }

    /// <summary>
    /// Removes a ticket booking.
    /// </summary>
    /// <param name="bookingSignalModel">The model containing the booking request information.</param>
    [WorkflowSignal]
    public async Task BookCompensationAsync(BookingSignalModel bookingSignalModel)
    {
        var index = _flight.Registered.FindIndex(s => s.Id == bookingSignalModel.Ticket.Id);
        
        _flight.Registered.RemoveAt(index);
    }

    /// <summary>
    /// Marks a ticket as paid.
    /// </summary>
    /// <param name="markTicketPaidSignalModel">The request model containing the ticket to be marked as paid.</param>
    [WorkflowSignal]
    public async Task MarkTicketPaidAsync(MarkTicketPaidSignalModel markTicketPaidSignalModel)
    {
        var ticket = _flight.Registered.Find(s => s.Id == markTicketPaidSignalModel.Ticket.Id);

        ticket.PaymentStatus = PaymentStatus.Paid;
    }

    /// <summary>
    /// Marks a ticket as cancelled.
    /// </summary>
    /// <param name="markTicketPaidSignalModel">The model containing information about the ticket to mark as paid.</param>
    [WorkflowSignal]
    public async Task MarkTicketPaidCompensationAsync(MarkTicketPaidSignalModel markTicketPaidSignalModel)
    {
        var ticket = _flight.Registered.Find(s => s.Id == markTicketPaidSignalModel.Ticket.Id);

        if (ticket is not null)
            ticket.PaymentStatus = PaymentStatus.Cancelled;
    }

    /// <summary>
    /// Reserves a seat for a ticket in the flight.
    /// </summary>
    /// <param name="seatReservationSignalModel">The model containing the seat and ticket information.</param>
    [WorkflowSignal]
    public async Task ReserveSeatAsync(SeatReservationSignalModel seatReservationSignalModel)
    {
        var seat = _flight.Seats.FirstOrDefault(f => f.Name == seatReservationSignalModel.Seat);

        seat.TicketId = seatReservationSignalModel.Ticket.Id;

        var ticket = _flight.Registered.FirstOrDefault(t => t.Id == seatReservationSignalModel.Ticket.Id);

        ticket.Seat = seat;
    }

    /// <summary>
    /// Removes seat reservation.
    /// </summary>
    /// <param name="seatReservationSignalModel">The seat reservation request model that contains the seat information.</param>
    [WorkflowSignal]
    public async Task ReserveSeatCompensationAsync(SeatReservationSignalModel seatReservationSignalModel)
    {
        var ticket = _flight.Registered.FirstOrDefault(t => t.Id == seatReservationSignalModel.Ticket.Id);

        ticket.Seat = null;

        var seat = _flight.Seats.FirstOrDefault(f => f.Name == seatReservationSignalModel.Seat);

        seat.TicketId = null;
    }

    /// <summary>
    /// Adds a passenger's ticket to the boarded list of a flight.
    /// </summary>
    /// <param name="boardingSignalModel">The model containing the information of the passenger's boarding request.</param>
    [WorkflowSignal]
    public async Task BoardPassengerAsync(BoardingSignalModel boardingSignalModel)
    {
        _flight.Boarded.Add(boardingSignalModel.Ticket);
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
}
