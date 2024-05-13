using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Interfaces.Purchase;

public interface ITicketService
{
    Task<List<Ticket>> GetTickets(Guid userId);

    Task<List<Ticket>> GetTickets(Guid userId, Guid flightId);

    Task<List<TicketWithCode>> GetPurchaseTickets(string purchaseId);

    Task<List<TicketWithCode>> GetPurchasePaidTickets(string purchaseId);

    Task<Ticket> GetTicket(Guid ticketId);

    Task<TicketWithCode> GetTicketWithCode(Guid ticketId);
}