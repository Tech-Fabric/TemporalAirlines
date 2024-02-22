using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Web.ViewComponents.PurchaseDetails;

public class PurchaseDetailsViewModel
{
    public string? PurchaseId { get; set; }
    
    public List<TicketWithCode> Tickets { get; set; } = [];
}