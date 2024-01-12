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
    }
}