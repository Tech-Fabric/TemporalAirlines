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
        throw new NotImplementedException();
    }

    public ITicketRepository GetTicketRepository()
    {
        throw new NotImplementedException();
    }

    public IUserRepository GetUserRepository()
    {
        throw new NotImplementedException();
    }
}
