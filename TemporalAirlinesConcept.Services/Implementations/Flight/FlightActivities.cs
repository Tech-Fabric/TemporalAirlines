using AutoMapper;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Models.Flight;
using Temporalio.Activities;

namespace TemporalAirlinesConcept.Services.Implementations.Flight;

public class FlightActivities
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public FlightActivities(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    [Activity]
    public Task<FlightDetailsModel> AssignSeats(FlightDetailsModel flight)
    {
        foreach (var ticket in flight.Registered)
        {
            if (ticket.Seat is not null)
                continue;

            var seat = flight.Seats.FirstOrDefault(s => s.TicketId is null);

            ticket.Seat = seat.Name;

            seat.TicketId = ticket.Id;
        }

        return Task.FromResult(flight);
    }

    [Activity]
    public async Task<bool> SaveFlightDetails(FlightDetailsModel flightDetailsModel)
    {
        var flight = _mapper.Map<DAL.Entities.Flight>(flightDetailsModel);

        _unitOfWork.Repository<DAL.Entities.Flight>().Update(flight);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    [Activity]
    public async Task<bool> SavePurchaseTickets(SaveTicketsModel saveTicketsModel)
    {
        foreach (var ticket in saveTicketsModel.Tickets)
        {
            var seat = await _unitOfWork.Repository<Seat>()
                .FindAsync(x => x.FlightId == ticket.FlightId && x.Name == ticket.Seat);

            var ticketToCreate = new Ticket
            {
                Id = ticket.Id,
                FlightId = ticket.FlightId,
                UserId = ticket.UserId == Guid.Empty ? null : ticket.UserId,
                PurchaseId = ticket.PurchaseId,
                Seat = seat,
                PaymentStatus = PaymentStatus.Pending,
                BoardingStatus = ticket.BoardingStatus
            };

            _unitOfWork.Repository<Ticket>().Insert(ticketToCreate);
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    [Activity]
    public async Task<bool> SavePurchaseTicketsCompensation(SaveTicketsModel saveTicketsModel)
    {
        foreach (var ticket in saveTicketsModel.Tickets)
        {
            var ticketToDelete = await _unitOfWork.Repository<Ticket>()
                .FindAsync(x => x.Id == ticket.Id);

            if (ticketToDelete != null)
                _unitOfWork.Repository<Ticket>().Remove(ticketToDelete);
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}
