using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Workflows;

namespace TemporalAirlinesConcept.Services.Implementations.Flight;

[Workflow]
public class FlightWorkflow
{
    private readonly ActivityOptions _activityOptions = new ActivityOptions 
        { StartToCloseTimeout = TimeSpan.FromSeconds(30) };

    private string _flightId;
    
    [WorkflowRun]
    public async Task RunAsync(DAL.Entities.Flight flight)
    {
        _flightId = flight.Id;
        
        await ChangeStatusAtTimeAsync(flight, FlightStatus.CheckIn, flight.Depart.Subtract(TimeSpan.FromDays(1)));
        
        await ChangeStatusAtTimeAsync(flight, FlightStatus.Boarding, flight.Depart.Subtract(TimeSpan.FromHours(2)));
        
        await ChangeStatusAtTimeAsync(flight, FlightStatus.Closed, flight.Depart.Subtract(TimeSpan.FromMinutes(5)));
        
        await ChangeStatusAtTimeAsync(flight, FlightStatus.Departed, flight.Depart);

        await ChangeStatusAtTimeAsync(flight, FlightStatus.Arrived, flight.Arrival);
    }

    private async Task ChangeStatusAtTimeAsync(DAL.Entities.Flight flight, FlightStatus status, DateTime time)
    {
        var delay = time.Subtract(Workflow.UtcNow);
        
        if (delay > TimeSpan.Zero)
        {
            await Workflow.DelayAsync(delay);
        }
        
        flight.Status = status;
        
        await Workflow.ExecuteActivityAsync((FlightActivities act) => act.SaveFlightInfoAsync(flight), _activityOptions);
    }

    /// <summary>
    /// Mark passenger as registered for this flight by ticket id
    /// </summary>
    /// <param name="bookingRequestModel"></param>
    [WorkflowSignal]
    public async Task BookSeatAsync(BookingRequestModel bookingRequestModel)
    {
        await Workflow.ExecuteActivityAsync(
            (FlightActivities act) => act.BookSeatAsync(new BookingModel
                { FlightId = _flightId, TicketId = bookingRequestModel.TicketId }), _activityOptions);
    }

    /// <summary>
    /// Remove passenger from a registered passengers list by ticket id
    /// </summary>
    /// <param name="bookingRequestModel"></param>
    [WorkflowSignal]
    public async Task BookSeatCompensationAsync(BookingRequestModel bookingRequestModel)
    {
        await Workflow.ExecuteActivityAsync(
            (FlightActivities act) =>
                act.BookSeatCompensationAsync(new BookingModel { FlightId = _flightId, TicketId = bookingRequestModel.TicketId }),
            _activityOptions);
    }

    /// <summary>
    /// Check in passenger by chosen seat and ticket id
    /// </summary>
    /// <param name="seatReservationRequestModel"></param>
    [WorkflowSignal]
    public async Task ReserveSeatAsync(SeatReservationRequestModel seatReservationRequestModel)
    {
        await Workflow.ExecuteActivityAsync(
            (FlightActivities act) => act.ReserveSeatAsync(new SeatReservationModel
            {
                FlightId = _flightId, Seat = seatReservationRequestModel.Seat,
                TicketId = seatReservationRequestModel.TicketId
            }), _activityOptions);
    }

    /// <summary>
    /// Remove seat reservation
    /// </summary>
    /// <param name="seatReservationRequestModel"></param>
    public async Task ReserveSeatCompensationAsync(SeatReservationRequestModel seatReservationRequestModel)
    {
        await Workflow.ExecuteActivityAsync(
            (FlightActivities act) => act.ReserveSeatCompensationAsync(new SeatReservationModel
            {
                FlightId = _flightId, Seat = seatReservationRequestModel.Seat,
                TicketId = seatReservationRequestModel.TicketId
            }), _activityOptions);
    }

    /// <summary>
    /// Mark passenger as boarded by ticket id
    /// </summary>
    /// <param name="boardingRequestModel"></param>
    [WorkflowSignal]
    public async Task BoardPassengerAsync(BoardingRequestModel boardingRequestModel)
    {
        await Workflow.ExecuteActivityAsync(
            (FlightActivities act) => act.BoardPassengerAsync(new BoardingModel
                { FlightId = _flightId, TicketId = boardingRequestModel.TicketId }), _activityOptions);
    }
}
