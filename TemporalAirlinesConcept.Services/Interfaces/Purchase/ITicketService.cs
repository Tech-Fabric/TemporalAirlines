using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Interfaces.Purchase;

public interface ITicketService
{
    Task<List<DAL.Entities.Ticket>> PurchaseTicketAsync(PurchaseInputModel purchaseRequestModel);
}
