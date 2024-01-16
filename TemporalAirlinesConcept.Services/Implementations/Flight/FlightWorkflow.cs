using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;
using Temporalio.Workflows;

namespace TemporalAirlinesConcept.Services.Implementations.Flight;

[Workflow]
public class FlightWorkflow
{
    // init flight model, not going to change it till the arrived state
    private DAL.Entities.Flight _flight;

    // current status
    private FlightStatus _status;

    // link between seat and passenger (ticket)  |  fills while check-in
    private Dictionary<string, string> _seats;

    // list of passengers (tickets) | fills while booking
    private List<Ticket> _registered;

    // list of boarded passengers
    private List<string> _boarded;

    [WorkflowRun]
    public async Task<bool> RunAsync(DAL.Entities.Flight flight)
    {
        var options = new ActivityOptions { StartToCloseTimeout = TimeSpan.FromSeconds(30) };

        _flight = flight;

        _seats = flight.Seats;

        _registered = [];

        _boarded = [];

        _status = FlightStatus.Pending;

        var checkInDateTime = _flight.Depart.Subtract(TimeSpan.FromDays(1));

        var sleepDuration = checkInDateTime.Subtract(Workflow.UtcNow);

        if (sleepDuration > TimeSpan.Zero)
        {
            await Workflow.DelayAsync(sleepDuration);
        }

        _status = FlightStatus.CheckIn;

        var boardingDateTime = _flight.Depart.Subtract(TimeSpan.FromHours(2));

        sleepDuration = boardingDateTime.Subtract(Workflow.UtcNow);

        if (sleepDuration > TimeSpan.Zero)
        {
            await Workflow.DelayAsync(sleepDuration);
        }

        _status = FlightStatus.Boarding;

        var closeDateTime = _flight.Depart.Subtract(TimeSpan.FromMinutes(5));

        sleepDuration = closeDateTime.Subtract(Workflow.UtcNow);

        if (sleepDuration > TimeSpan.Zero)
        {
            await Workflow.DelayAsync(sleepDuration);
        }

        _status = FlightStatus.Closed;

        sleepDuration = _flight.Depart.Subtract(Workflow.UtcNow);

        if (sleepDuration > TimeSpan.Zero)
        {
            await Workflow.DelayAsync(sleepDuration);
        }

        _status = FlightStatus.Departed;

        sleepDuration = _flight.Arrival.Subtract(Workflow.UtcNow);

        if (sleepDuration > TimeSpan.Zero)
        {
            await Workflow.DelayAsync(sleepDuration);
        }

        _status = FlightStatus.Arrived;

        _flight.Status = _status;
        _flight.Seats = _seats;
        _flight.Boarded = _boarded;
        _flight.Registered = _registered.Select(reg => reg.Id).ToList();

        await Workflow.ExecuteActivityAsync((FlightActivities act) => act.SaveFlightInfoAsync(_flight), options);

        return true;
    }

    //Check in passenger
    [WorkflowSignal]
    public Task ReserveSeatAsync(string seat, string ticketId)
    {
        if (_status is not FlightStatus.CheckIn)
            throw new ApplicationException("Flight status is not check in.");

        if (!_flight!.Seats.ContainsValue(seat))
            throw new ApplicationException("Seat does not exist");

        if (_seats![seat] is not null)
            throw new ApplicationException("Seat is already reserved.");

        _seats[seat] = ticketId;

        return Task.CompletedTask;
    }

    //Board passenger
    [WorkflowSignal]
    public Task BoardPassengerAsync(string ticketId)
    {
        // + should we check for paid ticket status?

        if (_status != FlightStatus.Boarding) 
            throw new ApplicationException("Flight status is not boarding.");

        if (!_seats!.ContainsValue(ticketId)) 
            throw new ApplicationException("Ticket ID is not valid.");

        if (_boarded!.Contains(ticketId)) 
            throw new ApplicationException("Passenger is already boarded.");

        _boarded.Add(ticketId);

        return Task.CompletedTask;
    }

    //Reserve seats
    [WorkflowSignal]
    public Task BookSeatsAsync(IEnumerable<Ticket> tickets)
    {
        if (_status != FlightStatus.Pending || _status != FlightStatus.Boarding)
            throw new ApplicationException("Registration for a flight is being closed");

        var enumerable = tickets as Ticket[] ?? tickets.ToArray();

        if (!enumerable.Any()) 
            return Task.CompletedTask;

        if (enumerable.Length + _registered!.Count > _flight!.Seats.Count)
            throw new ApplicationException("Not enough free seats to reserve a flight");

        _registered.AddRange(enumerable);

        return Task.CompletedTask;
    }

    //Reserve seats compensation
    [WorkflowSignal]
    public Task RemoveSeatsBookingAsync(IEnumerable<Ticket> tickets)
    {
        if (_status != FlightStatus.Pending || _status != FlightStatus.Boarding)
            throw new ApplicationException("Registration for a flight is being closed");

        _registered!.RemoveAll(t => tickets.Any(ticket => ticket.Id == t.Id));

        return Task.CompletedTask;
    }

    //Mark ticket as paid
    [WorkflowSignal]
    public Task MarkTicketAsPaidAsync(string userId, string passenger)
    {
        var ticket = _registered!.FirstOrDefault(t =>
            t.UserId == userId && t.Passenger == passenger && t.FlightId == _flight!.Id);

        if (ticket is null) 
            throw new ApplicationException("Ticket was not found");

        ticket.PaymentStatus = PaymentStatus.Paid;

        return Task.CompletedTask;
    }

    //Get flight status
    [WorkflowQuery]
    public FlightStatus GetStatus()
    {
        return _status;
    }

    //Retrieve number of available seats
    [WorkflowQuery]
    public int AvailableSeats()
    {
        return _flight!.Seats.Count - _registered!.Count;
    }

    //Retrieve seats reservations details
    [WorkflowQuery]
    public Dictionary<string, string> SeatsDetails()
    {
        return _seats!;
    }

    //Retrieve list of registered passengers
    [WorkflowQuery]
    public List<Ticket> RegisteredPassengers()
    {
        return _registered!;
    }

    //Retrieve list of boarded passengers
    [WorkflowQuery]
    public List<string> BoardedPassengers()
    {
        return _boarded!;
    }
}
