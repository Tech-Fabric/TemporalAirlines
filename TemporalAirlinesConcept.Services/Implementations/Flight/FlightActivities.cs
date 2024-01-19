using AutoMapper;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Models.Flight;
using Temporalio.Activities;

namespace TemporalAirlinesConcept.Services.Implementations.Flight;

public class FlightActivities
{
    private readonly IFlightRepository _flightRepository;
    private readonly IMapper _mapper;

    public FlightActivities(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _flightRepository = unitOfWork.GetFlightRepository();
        _mapper = mapper;
    }

    [Activity]
    public async Task<FlightDetailsModel> MapFlightModelAsync(DAL.Entities.Flight flight)
    {
        return _mapper.Map<FlightDetailsModel>(flight);
    }

    [Activity]
    public async Task<bool> SaveFlightDetailsAsync(FlightDetailsModel flightDetailsModel)
    {
        var flight = _mapper.Map<DAL.Entities.Flight>(flightDetailsModel);
        
        await _flightRepository.UpdateFlightAsync(flight);

        return true;
    }
}
