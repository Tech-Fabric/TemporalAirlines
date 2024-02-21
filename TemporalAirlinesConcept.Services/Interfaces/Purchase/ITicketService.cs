using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Interfaces.Purchase;

public interface ITicketService
{
    Task<string> StartTicketPurchase(PurchaseModel purchaseModel);

    Task<List<Ticket>> GetTickets(string userId);

    Task<List<Ticket>> GetTickets(string userId, string flightId);

    Task<List<TicketWithCode>> GetPurchaseWorkflowTickets(PurchaseTicketsRequestModel purchaseTicketsRequestModel);

    Task<Ticket> GetTicket(string ticketId);

    Task MarkAsPaid(string purchaseWorkflowId);

    Task<bool> RequestSeatReservation(SeatReservationInputModel seatReservationInputModel);

    Task<bool> BoardPassenger(BoardingInputModel boardingInputModel);
}
