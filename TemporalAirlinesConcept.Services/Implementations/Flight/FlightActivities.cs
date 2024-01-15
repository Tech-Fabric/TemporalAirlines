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
}
