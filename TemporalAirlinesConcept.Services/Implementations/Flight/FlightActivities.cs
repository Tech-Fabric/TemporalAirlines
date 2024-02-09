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
    public Task<FlightDetailsModel> MapFlightModel(DAL.Entities.Flight flight)
    {
        return Task.FromResult(_mapper.Map<FlightDetailsModel>(flight));
    }

    [Activity]
    public Task<FlightDetailsModel> AssignSeats(FlightDetailsModel flight)
    {
        foreach (var ticket in flight.Registered)
        {
            if (ticket.Seat is not null)
                continue;

            var seat = flight.Seats.FirstOrDefault(s => s.TicketId is null);

            ticket.Seat = seat;

            seat.TicketId = ticket.Id;
        }

        return Task.FromResult(flight);
    }

    [Activity]
    public async Task<bool> SaveFlightDetails(FlightDetailsModel flightDetailsModel)
    {
        var flight = _mapper.Map<DAL.Entities.Flight>(flightDetailsModel);

        await _flightRepository.UpdateFlightAsync(flight);

        return true;
    }
}
