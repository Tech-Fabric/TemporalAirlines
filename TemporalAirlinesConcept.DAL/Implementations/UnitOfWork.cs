using TemporalAirlinesConcept.DAL.Interfaces;

namespace TemporalAirlinesConcept.DAL.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly IFlightRepository _flightRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;

    public UnitOfWork(IFlightRepository flightRepository, ITicketRepository ticketRepository, IUserRepository userRepository)
    {
        _flightRepository = flightRepository;
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
    }

    public IFlightRepository GetFlightRepository()
    {
        return _flightRepository;
    }

    public ITicketRepository GetTicketRepository()
    {
        return _ticketRepository;
    }

    public IUserRepository GetUserRepository()
    {
        return _userRepository;
    }
}
