using AutoMapper;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Flight;

namespace TemporalAirlinesConcept.Services.Profiles;

public class FlightProfile : Profile
{
    public FlightProfile()
    {
        CreateMap<List<string>, Dictionary<string, string>>().ConvertUsing<ListToDictionaryConverter>();

        CreateMap<FlightInputModel, Flight>();

        CreateMap<Flight, FlightDetailsModel>()
            .ForMember(dest => dest.Seats,
                opt => 
                    opt.MapFrom(src => src.Seats.ToDictionary(s =>
                        s.Key, s => (Ticket)null)));

        CreateMap<FlightDetailsModel, Flight>()
            .ForMember(dest => dest.Seats,
                opt =>
                    opt.MapFrom(src =>
                        src.Seats.ToDictionary(s =>
                            s.Key, s => s.Value == null ? null : s.Value)))
            .ForMember(dest => dest.Registered,
                opt =>
                    opt.MapFrom(src => src.Registered.Select(s => s.Id)))
            .ForMember(dest => dest.Boarded, 
                opt =>
                opt.MapFrom(src => src.Boarded.Select(s => s.Id)));

    }
}