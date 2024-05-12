namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class SeatReservationSignalModel
{
    public SeatReservationSignalModel()
    {

    }

    public SeatReservationSignalModel(Guid ticketId, string seat)
    {
        TicketId = ticketId;
        Seat = seat;
    }

    public Guid TicketId { get; set; }

    public string Seat { get; set; }
}