using System.ComponentModel.DataAnnotations;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Web.ViewComponents.FlightBookingForm;

public class CreditCardDetails
{
    public string NameOnCard { get; set; }

    [CreditCard]
    public string CardNumber { get; set; }

    public int ExpiresMonth { get; set; }
    public int ExpiresYear { get; set; }
    public int CVV { get; set; }
}

public class FlightBookingFormViewModel
{
    public string? DepartureAirport { get; set; }

    public string? ArrivalAirport { get; set; }

    public DateTime? Departing { get; set; }
    
    public DateTime? ReturnDate { get; set; }

    public List<Flight>? Flights { get; set; }

    public Dictionary<string, string>? Airports { get; set; }

    public int NumberOfSeats { get; set; }

    public CreditCardDetails? CreditCardDetails { get; set; }

    public Guid? SelectedFlight { get; set; }

    public bool PaymentSuccessful { get; set; }

    public List<TicketWithCode> Tickets { get; set; } = [];

    public Dictionary<string, bool>? SelectedSeats { get; set; } = new();

    public string? PurchaseId { get; set; }

    public bool IsConfirmed { get; set; }

    public bool IsPaymentEmulated { get; set; }
}
