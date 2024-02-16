namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class SeatReservationSignalModel
{
    public SeatReservationSignalModel()
    {

    }

    public SeatReservationSignalModel(string ticketId, string seat)
    {
        TicketId = ticketId;
        Seat = seat;
    }

    public string TicketId { get; set; }

    public string Seat { get; set; }
}