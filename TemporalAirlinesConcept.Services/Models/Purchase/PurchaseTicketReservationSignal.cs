using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Services.Models.Purchase
{
    public class PurchaseTicketReservationSignal
    {
        public List<SeatReservationSignalModel> SeatReservations { get; set; }
        
        public List<Ticket> Tickets { get; set; }

        public string FlightId { get; set; }
    }
}
