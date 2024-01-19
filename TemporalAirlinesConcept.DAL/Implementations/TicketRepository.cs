using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using TemporalAirlinesConcept.Common.Settings;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Implementations;

public class TicketRepository : DbAccessService<Ticket>, ITicketRepository
{
    public TicketRepository(CosmosClient cosmosClient, IOptions<DatabaseSettings> options)
        : base(cosmosClient, options.Value.DbName, Ticket.Container)
    {
    }

    public Task<Ticket> GetTicketAsync(string id)
    {
        return GetItemAsync(id);
    }

    public async Task<List<Ticket>> GetTicketsAsync()
    {
        var queryResult = await GetItemsAsync("select * from c");

        return queryResult.ToList();
    }

    public async Task<List<Ticket>> QueryAsync(string query)
    {
        var queryResult = await GetItemsAsync(query);

        return queryResult.ToList();
    }

    public Task AddTicketAsync(Ticket ticket)
    {
        return AddItemAsync(ticket, ticket.Id);
    }

    public Task UpdateTicketAsync(Ticket ticket)
    {
        return UpdateItemAsync(ticket.Id, ticket);
    }

    public Task DeleteTicketAsync(string id)
    {
        return DeleteItemAsync(id);
    }
}