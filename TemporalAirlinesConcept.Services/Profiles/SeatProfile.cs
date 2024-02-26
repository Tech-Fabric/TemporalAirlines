using AutoMapper;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Profiles;

public class SeatProfile : Profile
{
    public SeatProfile()
    {
        CreateMap<SeatInputModel, Seat>()
            .ForMember(x => x.Flight, x => x.Ignore());

        CreateMap<Seat, SeatDetailsModel>();
    }
}
