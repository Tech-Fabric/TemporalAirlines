using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Interfaces.Purchase;

public interface ITicketService
{
    Task<string> StartTicketPurchase(PurchaseModel purchaseModel);

    Task<List<Ticket>> GetTickets(Guid userId);

    Task<List<Ticket>> GetTickets(Guid userId, Guid flightId);

    Task<List<TicketWithCode>> GetPurchaseWorkflowTickets(string purchaseId);

    Task<Ticket> GetTicket(Guid ticketId);
    Task<bool> IsPurchasePaid(string purchaseId);

    Task<bool> IsSeatsReserved(string purchaseId);
    
    Task<Ticket> GetTicket(string ticketId);

    Task<TicketWithCode> GetTicketWithCode(string ticketId);

    Task MarkAsPaid(string purchaseId);

    Task<bool> RequestSeatReservation(SeatReservationInputModel seatReservationInputModel);

    Task<bool> BoardPassenger(BoardingInputModel boardingInputModel);
}
