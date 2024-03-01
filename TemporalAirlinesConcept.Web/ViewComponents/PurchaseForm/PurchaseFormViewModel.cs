using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Web.ViewComponents.PurchaseForm;

public class PurchaseFormViewModel
{
    public Flight? Flight { get; set; }

    public string? PurchaseId { get; set; }

    public List<TicketWithCode> Tickets { get; set; } = [];

    public Dictionary<string, bool>? SelectedSeats { get; set; } = new();

    public bool IsReservedAndPaid { get; set; }

    public int NumberOfTickets { get; set; }

    public bool IsPurchaseRunning { get; set; }
}