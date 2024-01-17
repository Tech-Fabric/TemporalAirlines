using TemporalAirlinesConcept.DAL.Enums;
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
        
        var checkInDateTime = flight.Depart.Subtract(TimeSpan.FromDays(1));

        await DelayAsync(checkInDateTime.Subtract(Workflow.UtcNow));

        await ChangeStatusAndSaveAsync(flight, FlightStatus.CheckIn, _activityOptions);

        var boardingDateTime = flight.Depart.Subtract(TimeSpan.FromHours(2));

        await DelayAsync(boardingDateTime.Subtract(Workflow.UtcNow));

        await ChangeStatusAndSaveAsync(flight, FlightStatus.Boarding, _activityOptions);

        var closeDateTime = flight.Depart.Subtract(TimeSpan.FromMinutes(5));

        await DelayAsync(closeDateTime.Subtract(Workflow.UtcNow));

        await ChangeStatusAndSaveAsync(flight, FlightStatus.Closed, _activityOptions);

        await DelayAsync(flight.Depart.Subtract(Workflow.UtcNow));

        await ChangeStatusAndSaveAsync(flight, FlightStatus.Departed, _activityOptions);

        await DelayAsync(flight.Arrival.Subtract(Workflow.UtcNow));

        await ChangeStatusAndSaveAsync(flight, FlightStatus.Arrived, _activityOptions);
    }

    private static async Task DelayAsync(TimeSpan delay)
    {
        if (delay > TimeSpan.Zero)
        {
            await Workflow.DelayAsync(delay);
        }
    }

    private static async Task ChangeStatusAndSaveAsync(DAL.Entities.Flight flight, FlightStatus status, ActivityOptions options)
    {
        flight.Status = status;
        
        await Workflow.ExecuteActivityAsync((FlightActivities act) => act.SaveFlightInfoAsync(flight), options);
    }
    
    /// <summary>
    /// Mark passenger as registered for this flight by ticket id
    /// </summary>
    /// <param name="ticketId"></param>
    [WorkflowSignal]
    public async Task BookSeatAsync(string ticketId)
    {
        await Workflow.ExecuteActivityAsync((FlightActivities act) => act.BookSeatAsync(_flightId, ticketId),
            _activityOptions);
    }
    
    /// <summary>
    /// Remove passenger from a registered passengers list by ticket id
    /// </summary>
    /// <param name="ticketId"></param>
    [WorkflowSignal]
    public async Task BookSeatCompensationAsync(string ticketId)
    {
        await Workflow.ExecuteActivityAsync((FlightActivities act) => act.BookSeatCompensationAsync(_flightId, ticketId),
            _activityOptions);
    }
    
    /// <summary>
    /// Check in passenger by chosen seat and ticket id
    /// </summary>
    /// <param name="seat"></param>
    /// <param name="ticketId"></param>
    [WorkflowSignal]
    public async Task ReserveSeatAsync(string seat, string ticketId)
    {
        await Workflow.ExecuteActivityAsync((FlightActivities act) =>
            act.ReserveSeatAsync(_flightId, seat, ticketId), _activityOptions);
    }

    /// <summary>
    /// Remove seat reservation
    /// </summary>
    /// <param name="seat"></param>
    public async Task ReserveSeatCompensationAsync(string seat)
    {
        await Workflow.ExecuteActivityAsync((FlightActivities act) => act.ReserveSeatCompensationAsync(_flightId, seat),
            _activityOptions);
    }

    /// <summary>
    /// Mark passenger as boarded by ticket id
    /// </summary>
    /// <param name="ticketId"></param>
    [WorkflowSignal]
    public async Task BoardPassengerAsync(string ticketId)
    {
        await Workflow.ExecuteActivityAsync((FlightActivities act) => act.BoardPassengerAsync(_flightId, ticketId),
            _activityOptions);
    }
}
