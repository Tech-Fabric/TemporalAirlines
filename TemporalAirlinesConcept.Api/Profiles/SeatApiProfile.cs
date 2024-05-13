using AutoMapper;
using TemporalAirlinesConcept.Api.Models.Flights;
using TemporalAirlinesConcept.DAL.Entities;

namespace TemporalAirlinesConcept.Api.Profiles;

public class SeatApiProfilee : Profile
{
    public SeatApiProfilee()
    {
        CreateMap<Seat, SeatResponse>();
    }
}
