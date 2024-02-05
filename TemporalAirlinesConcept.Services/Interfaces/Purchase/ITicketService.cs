using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Interfaces.Purchase;

public interface ITicketService
{
    Task<string> RequestTicketPurchaseAsync(PurchaseModel purchaseModel);
    
    Task<bool> RequestSeatReservationAsync(SeatReservationInputModel seatReservationInputModel);

    Task<bool> BoardPassengerAsync(BoardingInputModel boardingInputModel);
}
