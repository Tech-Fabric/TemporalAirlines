using AutoMapper;
using TemporalAirlinesConcept.Common.Exceptions;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Activities;

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
        public async Task<bool> IsFlightAvailableAsync(List<string> flightsId)
        {
            foreach (var flightId in flightsId)
            {
                var flight = await _flightRepository.GetFlightAsync(flightId);

                if (flight.Seats.Count - flight.Registered.Count < 1) return false;
            }

            return true;
        }

        [Activity]
        public async Task CreateTicketAsync(Ticket ticket)
        {
            await _ticketRepository.AddTicketAsync(ticket);
        }
        
        [Activity]
        public async Task CreateTicketCompensationAsync(Ticket ticket)
        {
            await _ticketRepository.DeleteTicketAsync(ticket.Id);
        }
        
        [Activity]
        public async Task HoldMoneyAsync()
        {
            //send request to payment service to hold money
        }

        [Activity]
        public async Task HoldMoneyCompensationAsync()
        {
            //send request to payment service to release the money
        }

        [Activity]
        public async Task MarkTicketPaidAsync(string ticketId)
        {
            var ticket = await _ticketRepository.GetTicketAsync(ticketId);
            
            ticket.PaymentStatus = PaymentStatus.Paid;
            
            await _ticketRepository.UpdateTicketAsync(ticket);
        }
        
        [Activity]
        public async Task<List<TicketBlobModel>> GenerateTicketsAsync()
        {
            return [];
        }

        [Activity]
        public async Task GenerateTicketsCompensationAsync()
        {
            //remove tickets from blob
        }

        [Activity]
        public async Task SendTicketsAsync()
        {
            //retrieve user mail from db and send tickets
        }

        [Activity]
        public async Task SendTicketsCompensationAsync()
        {
            //notify client about tickets cancellation
        }
        

        [Activity]
        public async Task ConfirmWithdrawAsync()
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
