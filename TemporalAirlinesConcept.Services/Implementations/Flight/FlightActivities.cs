using AutoMapper;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Models.Flight;
using Temporalio.Activities;

namespace TemporalAirlinesConcept.Services.Implementations.Flight;

public class FlightActivities
{
    private readonly IMapper _mapper;
    private readonly IFlightRepository _flightRepository;
    private readonly ITicketRepository _ticketRepository;

    public FlightActivities(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _mapper = mapper;
        _flightRepository = unitOfWork.GetFlightRepository();
        _ticketRepository = unitOfWork.GetTicketRepository();
    }

    [Activity]
    public Task<FlightDetailsModel> MapFlightModel(DAL.Entities.Flight flight)
    {
        var flightDetail = _mapper.Map<FlightDetailsModel>(flight);

        return Task.FromResult(flightDetail);
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

    [Activity]
    public async Task<bool> SavePurchaseTickets(SaveTicketsModel saveTicketsModel)
    {
        foreach (var ticket in saveTicketsModel.Tickets)
        {
            await _ticketRepository.AddTicketAsync(ticket);
        }

        return true;
    }

    [Activity]
    public async Task<bool> SavePurchaseTicketsCompensation(SaveTicketsModel saveTicketsModel)
    {
        foreach (var ticket in saveTicketsModel.Tickets)
        {
            var ticketToDelete = await _ticketRepository.GetTicketAsync(ticket.Id);

            if (ticketToDelete != null)
                await _ticketRepository.DeleteTicketAsync(ticket.Id);
        }

        return true;
    }
}
