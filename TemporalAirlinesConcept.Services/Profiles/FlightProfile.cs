using AutoMapper;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.Services.Models.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;

namespace TemporalAirlinesConcept.Services.Profiles;

public class FlightProfile : Profile
{
    public FlightProfile()
    {
        CreateMap<SeatInputModel, Seat>();
        
        CreateMap<FlightInputModel, Flight>();

        CreateMap<Flight, FlightDetailsModel>();

        CreateMap<FlightDetailsModel, Flight>();
    }
}