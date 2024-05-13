using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Flight;

namespace TemporalAirlinesConcept.Services.Models.Purchase;

public class BookingSignalModel
{
    public BookingSignalModel()
    {

    }

    public BookingSignalModel(TicketDetailsModel ticket)
    {
        Ticket = ticket;
    }

    public TicketDetailsModel Ticket { get; set; }
}