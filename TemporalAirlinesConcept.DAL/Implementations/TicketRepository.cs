using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using TemporalAirlinesConcept.Common.Settings;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Implementations;

public class TicketRepository : ITicketRepository
{
    private readonly IDbAccessService<Ticket> _ticketDbService;

    public TicketRepository(CosmosClient cosmosClient, IOptions<DatabaseSettigns> options)
    {
        _ticketDbService = new DbAccessService<Ticket>(cosmosClient,
            options.Value.DbName, Ticket.Container);
    }

    public Task<Ticket?> GetTicketAsync(string id)
    {
        return _ticketDbService.GetItemAsync(id);
    }

    public async Task<List<Ticket>> GetTicketsAsync()
    {
        var queryResult = await _ticketDbService.GetItemsAsync("select * from c");

        return queryResult.ToList();
    }

    public async Task<List<Ticket>> QueryAsync(string query)
    {
        var queryResult = await _ticketDbService.GetItemsAsync(query);

        return queryResult.ToList();
    }

    public Task AddTicketAsync(Ticket ticket)
    {
        return _ticketDbService.AddItemAsync(ticket, ticket.Id);
    }

    public Task UpdateTicketAsync(Ticket ticket)
    {
        return _ticketDbService.UpdateItemAsync(ticket.Id, ticket);
    }

    public Task DeleteTicketAsync(string id)
    {
        return _ticketDbService.DeleteItemAsync(id);
    }
}