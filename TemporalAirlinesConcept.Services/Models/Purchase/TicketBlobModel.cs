namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class TicketBlobModel
{
    public string Id { get; set; }

    public string Passenger { get; set; }

    public string PassportNumber { get; set; }

    public string From { get; set; }

    public string To { get; set; }

    public DateTime Depart { get; set; }

    public DateTime Arrival { get; set; }

    public string FlightNumber { get; set; }

    public string Seat { get; set; }
}