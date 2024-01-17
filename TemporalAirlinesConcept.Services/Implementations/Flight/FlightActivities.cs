using TemporalAirlinesConcept.DAL.Interfaces;
using Temporalio.Activities;

namespace TemporalAirlinesConcept.Services.Implementations.Flight;

public class FlightActivities
{
    private readonly IFlightRepository _flightRepository;

    public FlightActivities(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }
    
    [Activity]
    public Task SaveFlightInfoAsync(DAL.Entities.Flight flight)
    {
        return _flightRepository.UpdateFlightAsync(flight);
    }

    [Activity]
    public async Task BookSeatAsync(string flightId, string ticketId)
    {
        var flight = await _flightRepository.GetFlightAsync(flightId);
        
        flight.Registered.Add(ticketId);
        
        await _flightRepository.UpdateFlightAsync(flight);
    }

    [Activity]
    public async Task BookSeatCompensationAsync(string flightId, string ticketId)
    {
        var flight = await _flightRepository.GetFlightAsync(flightId);

        flight.Registered.Remove(ticketId);

        await _flightRepository.UpdateFlightAsync(flight);
    }
    
    [Activity]
    public async Task ReserveSeatAsync(string flightId, string seat, string ticketId)
    {
        var flight = await _flightRepository.GetFlightAsync(flightId);

        flight.Seats[seat] = ticketId;
        
        await _flightRepository.UpdateFlightAsync(flight);
    }

    [Activity]
    public async Task ReserveSeatCompensationAsync(string flightId, string seat)
    {
        var flight = await _flightRepository.GetFlightAsync(flightId);

        flight.Seats[seat] = null;

        await _flightRepository.UpdateFlightAsync(flight);
    }

    [Activity]
    public async Task BoardPassengerAsync(string flightId, string ticketId)
    {
        var flight = await _flightRepository.GetFlightAsync(flightId);
        
        flight.Boarded.Add(ticketId);
        
        await _flightRepository.UpdateFlightAsync(flight);
    }
}
