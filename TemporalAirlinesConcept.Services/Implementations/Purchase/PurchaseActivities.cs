using AutoMapper;
using TemporalAirlinesConcept.Common.Exceptions;
using TemporalAirlinesConcept.Common.Helpers;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Activities;
using Temporalio.Client;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase
{
    public class PurchaseActivities
    {
        private readonly IMapper _mapper;
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRepository _ticketRepository;

        public PurchaseActivities(IMapper mapper, IFlightRepository flightRepository, ITicketRepository ticketRepository)
        {
            _mapper = mapper;
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
        }
        
        [Activity]
        public async Task HoldMoneyAsync(string userId, IEnumerable<string> flightsId)
        {
            //send request to payment service to hold money
        }

        [Activity]
        public async Task HoldMoneyCompensationAsync(string userId, IEnumerable<string> flightsId)
        {
            //send request to payment service to release the money
        }
        
        [Activity]
        public async Task<List<TicketBlobModel>> GenerateTicketsAsync(string userId, IEnumerable<string> flightsId, string passenger)
        {
            var tickets = new List<TicketBlobModel>();

            foreach (var flightId in flightsId)
            {
                var flight = await _flightRepository.GetFlightAsync(flightId);

                if (flight is null) throw new EntityNotFoundException("Flight was not found.");

                var ticket = new TicketBlobModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Passenger = passenger,
                    From = flight.From,
                    To = flight.To,
                    Depart = flight.Depart,
                    Arrival = flight.Arrival,
                    FlightNumber = flight.Id
                };

                tickets.Add(ticket);
            }

            // + save tickets to blob storage

            return tickets;
        }

        [Activity]
        public async Task GenerateTicketsCompensationAsync(IEnumerable<TicketBlobModel> list)
        {
            //remove tickets from blob
        }

        [Activity]
        public async Task SendTicketsAsync(string userId, IEnumerable<TicketBlobModel> tickets)
        {
            //retrieve user mail from db and send tickets
        }

        [Activity]
        public async Task SendTicketsCompensationAsync(string userId, IEnumerable<TicketBlobModel> tickets)
        {
            //notify client about tickets cancellation
        }

        [Activity]
        public async Task<List<Ticket>> SaveTicketsAsync(List<TicketBlobModel> tickets)
        {
            var dbTickets = _mapper.Map<List<Ticket>>(tickets);

            foreach (var ticket in dbTickets)
            {
                await _ticketRepository.AddTicketAsync(ticket);
            }

            return dbTickets;
        }

        [Activity]
        public async Task SaveTicketsCompensationAsync(List<Ticket> tickets)
        {
            foreach (var ticket in tickets)
            {
                await _ticketRepository.DeleteTicketAsync(ticket.Id);
            }
        }

        [Activity]
        public async Task ConfirmWithdrawAsync(string userId, IEnumerable<string> flightsId)
        {
            //send request to payment service to confirm a withdrawal
        }

        [Activity]
        public async Task<DAL.Entities.Flight> GetLastFlightAsync(string[] flightsId)
        {
            if (flightsId is null || flightsId.Length < 1)
                throw new ApplicationException("flights list is empty!");

            var lastFlight = await _flightRepository.GetFlightAsync(flightsId.LastOrDefault()!);

            if (lastFlight is null)
                throw new EntityNotFoundException("Flight was not found.");

            return lastFlight;
        }
    }
}
