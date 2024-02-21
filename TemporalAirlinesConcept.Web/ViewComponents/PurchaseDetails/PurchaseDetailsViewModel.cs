using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Web.ViewComponents.PurchaseDetails;

public class PurchaseDetailsViewModel
{
    public string? PurchaseId { get; set; }
    
    public List<Ticket> Tickets { get; set; } = [];
}