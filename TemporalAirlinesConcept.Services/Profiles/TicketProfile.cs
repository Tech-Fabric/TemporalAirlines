using AutoMapper;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Profiles;

public class TicketProfile : Profile
{
    public TicketProfile()
    {
        CreateMap<TicketBlobModel, Ticket>();
    }
}