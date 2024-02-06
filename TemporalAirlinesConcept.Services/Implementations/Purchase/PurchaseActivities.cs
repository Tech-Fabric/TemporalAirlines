using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;
using Temporalio.Activities;
using Temporalio.Client;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase
{
    public class PurchaseActivities
    {
        private readonly ITemporalClient _temporalClient;
        private readonly IFlightRepository _flightRepository;
        private readonly ITicketRepository _ticketRepository;

        public PurchaseActivities(ITemporalClient temporalClient, IUnitOfWork unitOfWork)
        {
            _temporalClient = temporalClient;
            _flightRepository = unitOfWork.GetFlightRepository();
            _ticketRepository = unitOfWork.GetTicketRepository();
        }

        /// <summary>
        /// Checks whether the flights specified by the flight IDs are available for booking.
        /// </summary>
        /// <param name="flightsId">The list of flight IDs to check for availability.</param>
        /// <returns>Returns true if all the flights are available; otherwise, false.</returns>
        [Activity]
        public async Task<bool> IsFlightAvailableAsync(string flightId)
        {
            var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flightId);

            var flight = await flightHandle.QueryAsync(wf => wf.GetFlightDetails());

            if (string.Equals(flight.From, Airports.ErrorCode) || string.Equals(flight.To, Airports.ErrorCode))
                throw new Exception("Artificial error exception");

            if (flight.Seats.Count - flight.Registered.Count < 1)
                return false;

            return true;
        }

        /// <summary>
        /// Creates a ticket and books the corresponding flight.
        /// </summary>
        /// <param name="ticket">The ticket object to be created.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation. The task result indicates whether the ticket creation was successful or not.</returns>
        [Activity]
        public async Task<bool> CreateTicketAsync(Ticket ticket)
        {
            var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(ticket.FlightId);

            await flightHandle.SignalAsync(wf =>
                wf.BookAsync(new BookingRequestModel { Ticket = ticket }));

            return true;
        }

        /// <summary>
        /// Removes a ticket from the registered list of flight.
        /// </summary>
        /// <param name="ticket">The ticket object representing the ticket which needs to be removed.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the ticket was removed successfully or not.</returns>
        [Activity]
        public async Task<bool> CreateTicketCompensationAsync(Ticket ticket)
        {
            var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(ticket.FlightId);

            await flightHandle.SignalAsync(wf =>
                wf.BookCompensationAsync(new BookingRequestModel { Ticket = ticket }));

            return true;
        }

        [Activity]
        public async Task<bool> HoldMoneyAsync()
        {
            return true;
        }

        [Activity]
        public async Task<bool> HoldMoneyCompensationAsync()
        {
            return true;
        }

        /// <summary>
        /// Marks a ticket as paid.
        /// </summary>
        /// <param name="ticket">The ticket to mark as paid.</param>
        /// <returns>A boolean value indicating whether the ticket was marked as paid successfully.</returns>
        [Activity]
        public async Task<bool> MarkTicketPaidAsync(Ticket ticket)
        {
            var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(ticket.FlightId);

            await flightHandle.SignalAsync(wf =>
                wf.MarkTicketPaidAsync(new MarkTicketPaidRequestModel { Ticket = ticket }));

            return true;
        }

        /// <summary>
        /// Marks a ticket as canceled.
        /// </summary>
        /// <param name="ticket">The ticket to mark as canceled.</param>
        /// <returns>A boolean value indicating whether the tickets was marked as canceled successfully.</returns>
        [Activity]
        public async Task<bool> MarkTicketPaidCompensationAsync(Ticket ticket)
        {
            var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(ticket.FlightId);

            await flightHandle.SignalAsync(wf =>
                wf.MarkTicketPaidCompensationAsync(new MarkTicketPaidRequestModel { Ticket = ticket }));

            return true;
        }

        /// <summary>
        /// Saves tickets to blob.
        /// </summary>
        /// <returns>A boolean value indicating whether the tickets was generated successfully.</returns>
        [Activity]
        public async Task<List<TicketBlobModel>> GenerateBlobTicketsAsync()
        {
            return [];
        }

        /// <summary>
        /// Deletes tickets from blob.
        /// </summary>
        /// <returns>A boolean value indicating whether the tickets was removed successfully.</returns>
        [Activity]
        public async Task<bool> GenerateBlobTicketsCompensationAsync()
        {
            return true;
        }

        /// <summary>
        /// Sends tickets.
        /// </summary>
        /// <returns>A boolean value indicating whether the tickets was sent successfully.</returns>
        [Activity]
        public async Task<bool> SendTicketsAsync()
        {
            return true;
        }

        /// <summary>
        /// Sends tickets compensation.
        /// </summary>
        /// <returns>A boolean value indicating whether the tickets compensation was sent successfully.</returns>
        [Activity]
        public async Task<bool> SendTicketsCompensationAsync()
        {
            return true;
        }

        /// <summary>
        /// Saves a list of tickets.
        /// </summary>
        /// <param name="tickets">The list of tickets to be saved.</param>
        /// <returns>A boolean indicating whether the tickets were saved successfully.</returns>
        [Activity]
        public async Task<bool> SaveTicketsAsync(List<Ticket> tickets)
        {
            await Task.WhenAll(tickets.Select(ticket => _ticketRepository.AddTicketAsync(ticket)));

            return true;
        }

        /// <summary>
        /// Removes the tickets.
        /// </summary>
        /// <param name="tickets">The list of tickets to save compensation for.</param>
        /// <returns>The task result is true if the operation is successful; otherwise, false.</returns>
        [Activity]
        public async Task<bool> SaveTicketsCompensationAsync(List<Ticket> tickets)
        {
            await Task.WhenAll(tickets.Select(ticket => _ticketRepository.DeleteTicketAsync(ticket.Id)));

            return true;
        }

        /// <summary>
        /// Confirms a withdrawal.
        /// </summary>
        [Activity]
        public async Task<bool> ConfirmWithdrawAsync()
        {
            return true;
        }

        /// <summary>
        /// Retrieves the last flight from the given list of flight IDs.
        /// </summary>
        /// <param name="flightsId">The list of flight IDs.</param>
        [Activity]
        public async Task<DAL.Entities.Flight> GetFlightAsync(string flightId)
        {
            return await _flightRepository.GetFlightAsync(flightId);
        }
    }
}
