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
        private readonly ITemporalClient _temporalClient;

        public PurchaseActivities(IMapper mapper, IFlightRepository flightRepository, ITicketRepository ticketRepository, ITemporalClient temporalClient)
        {
            _mapper = mapper;
            _flightRepository = flightRepository;
            _ticketRepository = ticketRepository;
            _temporalClient = temporalClient;
        }

        [Activity]
        public async Task<bool> IsFlightExistsAsync(string flightId)
        {
            var flight = await _flightRepository.GetFlightAsync(flightId);

            return flight is not null;
        }

        [Activity]
        public async Task<bool> IsFlightsAvailableAsync(IEnumerable<string> flightsId)
        {
            foreach (var flightId in flightsId)
            {
                if (!await WorkflowHadleHelper.IsWorkflowExists<FlightWorkflow>(_temporalClient, flightId)) // todo: can not use temporal client in activity ???_))))))
                {
                    var flight = await _flightRepository.GetFlightAsync(flightId);

                    if (flight is null)
                        throw new EntityNotFoundException("Flight was not found.");

                    await _temporalClient.StartWorkflowAsync((FlightWorkflow wf) => wf.RunAsync(flight),
                        new WorkflowOptions(flightId, "flight-task-queue"));
                }

                var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId);

                var availableSeats = await handle.QueryAsync(wf => wf.AvailableSeats());

                if (availableSeats < 1) return false;
            }

            return true;
        }

        [Activity]
        public async Task BookFlightAsync(string userId, IEnumerable<string> flightsId, string passenger)
        {
            foreach (var flightId in flightsId)
            {
                if (!await WorkflowHadleHelper.IsWorkflowExists<FlightWorkflow>(_temporalClient, flightId))
                    throw new ApplicationException("Flight workflow does not exist");

                var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId);

                var ticket = new Ticket
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    FlightId = flightId,
                    Passenger = passenger,
                    PaymentStatus = PaymentStatus.Pending
                };

                await handle.SignalAsync(wf => wf.BookSeatsAsync(new[] { ticket }));
            }
        }

        [Activity]
        public async Task BookFlightCompensationAsync(string userId, IEnumerable<string> flightsId, string passenger)
        {
            foreach (var flightId in flightsId)
            {
                if (!await WorkflowHadleHelper.IsWorkflowExists<FlightWorkflow>(_temporalClient, flightId))
                    throw new ApplicationException("Flight workflow does not exist");

                var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId);

                var ticket = new Ticket
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    FlightId = flightId,
                    Passenger = passenger,
                    PaymentStatus = PaymentStatus.Pending
                };

                await handle.SignalAsync(wf => wf.RemoveSeatsBookingAsync(new[] { ticket }));
            }
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
        public async Task ConfirmPurchaseAsync(string userId, IEnumerable<string> flightsId, string passenger)
        {
            foreach (var flightId in flightsId)
            {
                if (!await WorkflowHadleHelper.IsWorkflowExists<FlightWorkflow>(_temporalClient, flightId))
                    throw new ApplicationException("Flight workflow does not exist");

                var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId);

                await handle.SignalAsync(wf => wf.MarkTicketAsPaidAsync(userId, passenger));
            }
        }

        [Activity]
        public async Task ConfirmPurchaseCompensationAsync(string userId, IEnumerable<string> flightsId, string passenger)
        {
            //no need to mark ticket as unpaid if we are going to delete it in the next compensation step (BookFlightCompensationAsync)?
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
