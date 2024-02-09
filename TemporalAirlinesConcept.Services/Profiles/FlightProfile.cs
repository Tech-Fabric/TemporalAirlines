using AutoMapper;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Models.Seat;
using TemporalAirlinesConcept.Services.Models.Flight;

namespace TemporalAirlinesConcept.Services.Profiles;

public class FlightProfile : Profile
{
    public FlightProfile()
    {
        CreateMap<SeatInputModel, Seat>();
        
        CreateMap<FlightInputModel, Flight>();

        CreateMap<Flight, FlightDetailsModel>();

        CreateMap<FlightDetailsModel, Flight>()
            .ForMember(dest => dest.Registered,
                opt =>
                    opt.MapFrom(src => src.Registered.Select(s => s.Id)))
            .ForMember(dest => dest.Boarded, 
                opt =>
                opt.MapFrom(src => src.Boarded.Select(s => s.Id)));

    }
}