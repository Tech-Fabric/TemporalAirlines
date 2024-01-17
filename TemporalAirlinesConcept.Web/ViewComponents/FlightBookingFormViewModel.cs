using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Web.ViewComponents;

public class FlightBookingFormViewModel
{
    public string? DepartureAirport { get; set; }
    public string? ArrivalAirport { get; set; }

    public DateTime? When { get; set; }

    public string? SelectedFlight { get; set; }

    public List<Flight>? Flights { get; set; }
    public Dictionary<string,string>? Airports { get; set; }
}
