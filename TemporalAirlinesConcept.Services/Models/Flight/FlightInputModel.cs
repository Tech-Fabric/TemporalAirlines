namespace TemporalAirlinesConcept.Services.Models.Flight;

public class FlightInputModel
{
    public string From { get; set; }

    public string To { get; set; }

    public DateTime Depart { get; set; }

    public DateTime Arrival { get; set; }

    public List<string> Seats { get; set; }
}