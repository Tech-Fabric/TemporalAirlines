using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.DAL.Interfaces;

public interface IFlightRepository
{
    Task<Flight> GetFlightAsync(string id);

    Task<List<Flight>> GetFlightsAsync();

    Task<List<Flight>> QueryAsync(string query);

    Task AddFlightAsync(Flight flight);

    Task UpdateFlightAsync(Flight flight);

    Task DeleteFlightAsync(string id);
}