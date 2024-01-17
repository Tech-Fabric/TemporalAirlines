using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Interfaces.Purchase;

public interface ITicketService
{
    Task RequestTicketPurchaseAsync(PurchaseInputModel purchaseRequestModel);
}
