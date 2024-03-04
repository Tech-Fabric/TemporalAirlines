using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Interfaces.Purchase;

public interface IPurchaseService
{
    Task<string> StartPurchase(PurchaseModel purchaseModel);
    
    Task<bool> IsPaid(string purchaseId);

    Task<bool> IsSeatsReserved(string purchaseId);

    Task<bool> IsReservedAndPaid(string purchaseId);

    Task MarkAsPaid(string purchaseId);

    Task RequestSeatReservation(SeatReservationInputModel seatReservationInputModel);

    Task ReserveAndPaySeats(SeatReservationInputModel seatReservationInputModel);

    Task<bool> BoardPassenger(BoardingInputModel boardingInputModel);

    Task<bool> IsPurchaseRunningOrCompleted(string purchaseId);
}