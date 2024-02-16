using AutoMapper;
using TemporalAirlinesConcept.Api.Models.Tickets;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Api.Profiles;

public class TicketApiProfile : Profile
{
    public TicketApiProfile()
    {
        CreateMap<DAL.Entities.Ticket, TicketResponse>();

        CreateMap<StartPurchaseRequest, PurchaseModel>();
    }
}
