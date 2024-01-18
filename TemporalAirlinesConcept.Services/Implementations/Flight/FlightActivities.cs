using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Activities;

namespace TemporalAirlinesConcept.Services.Implementations.Flight;

public class FlightActivities
{
    private readonly IFlightRepository _flightRepository;

    public FlightActivities(IFlightRepository flightRepository)
    {
        _flightRepository = flightRepository;
    }
    
    [Activity]
    public Task SaveFlightInfoAsync(DAL.Entities.Flight flight)
    {
        return _flightRepository.UpdateFlightAsync(flight);
    }

    [Activity]
    public async Task BookSeatAsync(BookingModel bookingModel)
    {
        var flight = await _flightRepository.GetFlightAsync(bookingModel.FlightId);
        
        flight.Registered.Add(bookingModel.TicketId);
        
        await _flightRepository.UpdateFlightAsync(flight);
    }

    [Activity]
    public async Task BookSeatCompensationAsync(BookingModel bookingModel)
    {
        var flight = await _flightRepository.GetFlightAsync(bookingModel.FlightId);

        flight.Registered.Remove(bookingModel.TicketId);

        await _flightRepository.UpdateFlightAsync(flight);
    }
    
    [Activity]
    public async Task ReserveSeatAsync(SeatReservationModel seatReservationModel)
    {
        var flight = await _flightRepository.GetFlightAsync(seatReservationModel.FlightId);

        flight.Seats[seatReservationModel.Seat] = seatReservationModel.TicketId;
        
        await _flightRepository.UpdateFlightAsync(flight);
    }

    [Activity]
    public async Task ReserveSeatCompensationAsync(SeatReservationModel seatReservationModel)
    {
        var flight = await _flightRepository.GetFlightAsync(seatReservationModel.FlightId);

        flight.Seats[seatReservationModel.Seat] = null;

        await _flightRepository.UpdateFlightAsync(flight);
    }

    [Activity]
    public async Task BoardPassengerAsync(BoardingModel boardingModel)
    {
        var flight = await _flightRepository.GetFlightAsync(boardingModel.FlightId);
        
        flight.Boarded.Add(boardingModel.TicketId);
        
        await _flightRepository.UpdateFlightAsync(flight);
    }
}
