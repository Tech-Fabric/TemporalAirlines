using TemporalAirlinesConcept.Api.Models.Flights;
using TemporalAirlinesConcept.DAL.Enums;

namespace TemporalAirlinesConcept.Api.Models.Tickets
{
    public class TicketResponse
    {
        public string Id { get; set; }

        public string FlightId { get; set; }

        public string UserId { get; set; }

        public string Passenger { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public SeatResponse Seat { get; set; }
    }
}
