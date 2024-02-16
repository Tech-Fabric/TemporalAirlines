using AutoMapper;
using TemporalAirlinesConcept.Api.Models.Flights;
using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Api.Profiles;

public class FlightApiProfile : Profile
{
    public FlightApiProfile()
    {
        CreateMap<Flight, FlightResponse>();
    }
}