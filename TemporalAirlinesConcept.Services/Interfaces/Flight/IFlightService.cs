using TemporalAirlinesConcept.Services.Models.Flight;

namespace TemporalAirlinesConcept.Services.Interfaces.Flight;

public interface IFlightService
{
    public Task<List<DAL.Entities.Flight>> GetFlightsAsync();

    public Task<DAL.Entities.Flight> GetFlightAsync(string id);

    public Task<DAL.Entities.Flight> CreateFlightAsync(FlightInputModel model);

    public Task RemoveFlightAsync(string id);
}