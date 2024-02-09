using TemporalAirlinesConcept.Services.Models.Flight;

namespace TemporalAirlinesConcept.Services.Interfaces.Flight;

public interface IFlightService
{
    public Task<List<DAL.Entities.Flight>> GetFlights();

    public Task<DAL.Entities.Flight> GetFlight(string id);

    public Task<DAL.Entities.Flight> FetchFlightDetailFromWorkflow(DAL.Entities.Flight flight);

    public Task<DAL.Entities.Flight> CreateFlight(FlightInputModel model);

    public Task RemoveFlight(string id);
}