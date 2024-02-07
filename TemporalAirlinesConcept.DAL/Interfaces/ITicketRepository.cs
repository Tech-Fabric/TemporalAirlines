using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.DAL.Interfaces;

public interface ITicketRepository
{
    Task<Ticket> GetTicketAsync(string id);

    Task<List<Ticket>> GetTicketsAsync();

    Task<List<Ticket>> QueryAsync(string query);

    Task<List<Ticket>> GetTicketsByUserIdAsync(string userId);
    
    Task<List<Ticket>> GetTicketsByUserIdFlightAsync(string userId, string flightId);

    Task AddTicketAsync(Ticket ticket);

    Task UpdateTicketAsync(Ticket ticket);

    Task DeleteTicketAsync(string id);
}