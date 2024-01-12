namespace TemporalAirlinesConcept.DAL.Interfaces;

public interface IUnitOfWork
{
    IFlightRepository GetFlightRepository();
    ITicketRepository GetTicketRepository();
    IUserRepository GetUserRepository();
}