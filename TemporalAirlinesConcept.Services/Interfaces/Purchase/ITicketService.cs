using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Interfaces.Purchase;

public interface ITicketService
{
    Task<string> RequestTicketPurchaseAsync(PurchaseModel purchaseModel);

    Task<List<Ticket>> GetTickets(string userId);

    Task<List<Ticket>> GetTickets(string userId, string flightId);

    Task<Ticket> GetTicket(string ticketId);

    Task MarkAsPaid(string purchaseWorkflowId);

    public Task SetSeatsSelection(string purchaseWorkflowId, List<string> selectedSeats);
    public Task SetPassengerDetails(string purchaseWorkflowId, List<string> passengerDetails);
}
