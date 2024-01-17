using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using TemporalAirlinesConcept.Common.Settings;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Implementations;

public class FlightRepository : DbAccessService<Flight>, IFlightRepository
{
    public FlightRepository(CosmosClient cosmosClient, IOptions<DatabaseSettings> options)
        : base(cosmosClient, options.Value.DbName, Flight.Container)
    {
    }

    public Task<Flight> GetFlightAsync(string id)
    {
        return GetItemAsync(id);
    }

    public async Task<List<Flight>> GetFlightsAsync()
    {
        var queryResult = await GetItemsAsync("select * from c");

        return queryResult.ToList();
    }

    public async Task<List<Flight>> QueryAsync(string query)
    {
        var queryResult = await GetItemsAsync(query);

        return queryResult.ToList();
    }

    public Task AddFlightAsync(Flight flight)
    {
        return AddItemAsync(flight, flight.Id);
    }

    public Task UpdateFlightAsync(Flight flight)
    {
        return UpdateItemAsync(flight.Id, flight);
    }

    public Task DeleteFlightAsync(string id)
    {
        return DeleteItemAsync(id);
    }
}