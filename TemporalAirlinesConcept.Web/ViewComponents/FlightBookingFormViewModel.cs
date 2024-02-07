using System.ComponentModel.DataAnnotations;
using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Web.ViewComponents;

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

    public DateTime? When { get; set; }

    public List<Flight>? Flights { get; set; }
    public Dictionary<string,string>? Airports { get; set; }

    public int NumberOfSeats { get; set; }

    public CreditCardDetails? CreditCardDetails { get; set; }

    public string? SelectedFlight { get; set; }

    public bool PaymentSuccessful { get; set; }

    public Dictionary<string,bool>? SelectedSeats { get; set; }

    public bool ShowSeatSelectionScreen { get; set; }
}
