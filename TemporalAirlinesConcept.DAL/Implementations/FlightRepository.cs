using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using TemporalAirlinesConcept.Common.Settings;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Implementations;

public class FlightRepository : IFlightRepository
{
    private readonly IDbAccessService<Flight> _flightDbService;

    public FlightRepository(CosmosClient cosmosClient, IOptions<DatabaseSettigns> options)
    {
        _flightDbService = new DbAccessService<Flight>(cosmosClient,
            options.Value.DbName, Flight.Container);
    }

    public Task<Flight> GetFlightAsync(string id)
    {
        return _flightDbService.GetItemAsync(id);
    }

    public async Task<List<Flight>> GetFlightsAsync()
    {
        var queryResult = await _flightDbService.GetItemsAsync("select * from c");

        return queryResult.ToList();
    }

    public async Task<List<Flight>> QueryAsync(string query)
    {
        var queryResult = await _flightDbService.GetItemsAsync(query);

        return queryResult.ToList();
    }

    public Task AddFlightAsync(Flight flight)
    {
        return _flightDbService.AddItemAsync(flight, flight.Id);
    }

    public Task UpdateFlightAsync(Flight flight)
    {
        return _flightDbService.UpdateItemAsync(flight.Id, flight);
    }

    public Task DeleteFlightAsync(string id)
    {
        return _flightDbService.DeleteItemAsync(id);
    }
}