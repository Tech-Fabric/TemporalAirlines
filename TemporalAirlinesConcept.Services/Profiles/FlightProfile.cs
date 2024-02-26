using AutoMapper;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Profiles;

public class FlightProfile : Profile
{
    public FlightProfile()
    {
        CreateMap<FlightInputModel, Flight>()
            .ForMember(x => x.Seats, x => x.MapFrom(y => y.Seats));

        CreateMap<Flight, FlightDetailsModel>()
            .ForMember(x => x.Seats, x => x.MapFrom(y => y.Seats));

        CreateMap<FlightDetailsModel, Flight>()
            .ForMember(x => x.Seats, x => x.MapFrom(y => y.Seats));
    }
}