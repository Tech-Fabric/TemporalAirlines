﻿using Microsoft.Extensions.Options;
using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Common.Extensions;
using TemporalAirlinesConcept.Common.Settings;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Implementations.QRCodeGeneration;
using TemporalAirlinesConcept.Services.Interfaces.Purchase;
using TemporalAirlinesConcept.Services.Models.Purchase;
using TemporalAirlinesConcept.Services.Models.QRCodeGeneration;
using Temporalio.Client;
using Temporalio.Common;

namespace TemporalAirlinesConcept.Services.Implementations.Purchase;

public class TicketService : ITicketService
{
    private readonly ITemporalClient _temporalClient;
    private readonly ITicketRepository _ticketRepository;
    private readonly UrlSettings _urlSettings;

    public TicketService(ITemporalClient temporalClient, ITicketRepository ticketRepository, IOptions<UrlSettings> urlSettings)
    {
        _temporalClient = temporalClient;
        _ticketRepository = ticketRepository;
        _urlSettings = urlSettings.Value;
    }

    public async Task<Ticket> GetTicket(string ticketId)
    {
        var ticket = await _ticketRepository.GetTicketAsync(ticketId);

        return ticket;
    }

    public async Task<List<Ticket>> GetTickets(string userId)
    {
        var tickets = await _ticketRepository.GetTicketsByUserIdAsync(userId);

        return tickets;
    }

    public async Task<List<Ticket>> GetTickets(string userId, string flightId)
    {
        var tickets = await _ticketRepository.GetTicketsByUserIdFlightAsync(userId, flightId);

        return tickets;
    }

    public async Task<List<TicketWithCode>> GetPurchaseWorkflowTickets(PurchaseTicketsRequestModel purchaseTicketsRequestModel)
    {
        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(purchaseTicketsRequestModel.FlightId))
            return [];

        var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(purchaseTicketsRequestModel.FlightId);

        var tickets = await handle.QueryAsync(wf => wf.GetRegisteredTickets());

        var ticketWithCode = tickets
            .Where(t => t.PurchaseId == purchaseTicketsRequestModel.PurchaseId)
            .Select(x => new TicketWithCode
            {
                Id = x.Id,
                PaymentStatus = x.PaymentStatus,
                Seat = x.Seat,
                Code = QRCodeGeneratorService.Generate(new QRDataModel
                {
                    Data = $"{_urlSettings.TicketPage}/{x.Id}"
                })
            })
            .ToList();

        return ticketWithCode;
    }

    public async Task MarkAsPaid(string purchaseWorkflowId)
    {
        var handle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(purchaseWorkflowId);

        await handle.SignalAsync(x => x.SetAsPaid());
    }

    public async Task<string> StartTicketPurchase(PurchaseModel purchaseModel)
    {
        var workflowId = Guid.NewGuid().ToString();

        await _temporalClient.StartWorkflowAsync<PurchaseWorkflow>(
            wf => wf.Run(purchaseModel), new WorkflowOptions
            {
                TaskQueue = Temporal.DefaultQueue,
                Id = workflowId,
                RetryPolicy = new RetryPolicy
                {
                    MaximumAttempts = 1,
                    InitialInterval = TimeSpan.FromMinutes(5),
                    BackoffCoefficient = 2
                }
            });

        return workflowId;
    }

    public async Task<bool> RequestSeatReservation(SeatReservationInputModel seatReservationInputModel)
    {
        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(seatReservationInputModel.FlightId)
            || !await _temporalClient.IsWorkflowRunning<PurchaseWorkflow>(seatReservationInputModel.PurchaseId))
            return false;

        var flightHandle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(seatReservationInputModel.FlightId);
        var purchaseHandle = _temporalClient.GetWorkflowHandle<PurchaseWorkflow>(seatReservationInputModel.PurchaseId);

        var registered = await flightHandle.QueryAsync(wf => wf.GetRegisteredTickets());

        var tickets = registered.Where(t =>
            t.PurchaseId == seatReservationInputModel.PurchaseId).ToList();

        var seatReservations = tickets.Select((t, i) =>
            new SeatReservationSignalModel { TicketId = t.Id, Seat = seatReservationInputModel.Seats[i] }).ToList();

        var signalModel = new PurchaseTicketReservationSignal
        {
            SeatReservations = seatReservations,
            FlightId = seatReservationInputModel.FlightId
        };

        await purchaseHandle.SignalAsync(x => x.TicketReservation(signalModel));

        return true;
    }

    public async Task<bool> BoardPassenger(BoardingInputModel boardingInputModel)
    {
        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(boardingInputModel.FlightId))
            return false;

        var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(boardingInputModel.FlightId);

        var tickets = await handle.QueryAsync(wf => wf.GetRegisteredTickets());

        var ticket = tickets.FirstOrDefault(t => t.Id == boardingInputModel.TicketId);

        if (ticket is null)
            return false;

        await handle.SignalAsync(wf => wf.BoardPassenger(new BoardingSignalModel
        {
            Ticket = ticket
        }));

        return true;
    }
}
