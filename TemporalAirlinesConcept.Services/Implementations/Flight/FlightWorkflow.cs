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
    /// <param name="bookingRequestModel">The booking request model.</param>
    [WorkflowSignal]
    public async Task BookAsync(BookingRequestModel bookingRequestModel)
    {
        _flight.Registered.Add(bookingRequestModel.Ticket);
    }

    /// <summary>
    /// Removes a ticket booking.
    /// </summary>
    /// <param name="bookingRequestModel">The model containing the booking request information.</param>
    [WorkflowSignal]
    public async Task BookCompensationAsync(BookingRequestModel bookingRequestModel)
    {
        var index = _flight.Registered.FindIndex(s => s.Id == bookingRequestModel.Ticket.Id);

        _flight.Registered.RemoveAt(index);
    }

    /// <summary>
    /// Marks a ticket as paid.
    /// </summary>
    /// <param name="markTicketPaidRequestModel">The request model containing the ticket to be marked as paid.</param>
    [WorkflowSignal]
    public async Task MarkTicketPaidAsync(MarkTicketPaidRequestModel markTicketPaidRequestModel)
    {
        var ticket = _flight.Registered.Find(s => s.Id == markTicketPaidRequestModel.Ticket.Id);

        ticket.PaymentStatus = PaymentStatus.Paid;
    }

    /// <summary>
    /// Marks a ticket as cancelled.
    /// </summary>
    /// <param name="markTicketPaidRequestModel">The model containing information about the ticket to mark as paid.</param>
    [WorkflowSignal]
    public async Task MarkTicketPaidCompensationAsync(MarkTicketPaidRequestModel markTicketPaidRequestModel)
    {
        var ticket = _flight.Registered.Find(s => s.Id == markTicketPaidRequestModel.Ticket.Id);

        if (ticket is not null)
            ticket.PaymentStatus = PaymentStatus.Cancelled;
    }

    /// <summary>
    /// Reserves a seat for a ticket in the flight.
    /// </summary>
    /// <param name="seatReservationRequestModel">The model containing the seat and ticket information.</param>
    [WorkflowSignal]
    public async Task ReserveSeatAsync(SeatReservationRequestModel seatReservationRequestModel)
    {
        _flight.Seats[seatReservationRequestModel.Seat] = seatReservationRequestModel.Ticket;
    }

    /// <summary>
    /// Removes seat reservation.
    /// </summary>
    /// <param name="seatReservationRequestModel">The seat reservation request model that contains the seat information.</param>
    public async Task ReserveSeatCompensationAsync(SeatReservationRequestModel seatReservationRequestModel)
    {
        _flight.Seats[seatReservationRequestModel.Seat] = null;
    }

    /// <summary>
    /// Adds a passenger's ticket to the boarded list of a flight.
    /// </summary>
    /// <param name="boardingRequestModel">The model containing the information of the passenger's boarding request.</param>
    [WorkflowSignal]
    public async Task BoardPassengerAsync(BoardingRequestModel boardingRequestModel)
    {
        _flight.Boarded.Add(boardingRequestModel.Ticket);
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
