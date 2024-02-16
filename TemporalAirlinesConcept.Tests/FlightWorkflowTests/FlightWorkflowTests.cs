using AutoMapper;
using FluentAssertions;
using Moq;
using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Enums;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using TemporalAirlinesConcept.Services.Models.Flight;
using TemporalAirlinesConcept.Services.Models.Purchase;
using TemporalAirlinesConcept.Services.Profiles;
using TemporalAirlinesConcept.Tests.Factories;
using TemporalAirlinesConcept.Tests.Helpers;
using Temporalio.Client;
using Temporalio.Common;
using Temporalio.Testing;

namespace TemporalAirlinesConcept.Tests.FlightWorkflowTests;

public class FlightWorkflowTests
{
    private readonly IMapper _mapper;

    private readonly RetryPolicy _retryPolicy =
        new()
        {
            MaximumAttempts = 1
        };

    public FlightWorkflowTests()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddProfile(new FlightProfile()); });

        _mapper = config.CreateMapper();
    }

    [Fact]
    public async Task FlightWorkflow_Succeeds()
    {
        // Arrange
        await using var env = await WorkflowEnvironment.StartTimeSkippingAsync();

        var flight = TestFlightFabric.GetTestFlight();

        var mockFlightRepository = new Mock<IFlightRepository>();

        using var worker = await WorkerHelper.ConfigureWorkerAsync(env, _mapper, mockFlightRepository);

        // Act
        var workflowExecution = async () => await worker.ExecuteAsync(async () =>
        {
            var handle = await env.Client.StartWorkflowAsync((FlightWorkflow wf) => wf.Run(flight),
                new WorkflowOptions { Id = flight.Id, TaskQueue = Temporal.DefaultQueue, RetryPolicy = _retryPolicy });

            await handle.GetResultAsync();
        });

        // Assert
        await workflowExecution.Should().NotThrowAsync();

        mockFlightRepository.Verify(x => x.UpdateFlightAsync(It.IsAny<Flight>()), Times.Once);
    }

    [Fact]
    public async Task RegisterPassengerPaymentTimeout_Succeeds()
    {
        // Arrange
        await using var env = await WorkflowEnvironment.StartTimeSkippingAsync();

        var flight = TestFlightFabric.GetTestFlight();

        var bookingRequestModel = new BookingSignalModel(TestTicketFabric.GetTestTicket(flight.Id));

        using var worker = await WorkerHelper.ConfigureWorkerAsync(env, _mapper);

        var flightDetailsModelWithTicket = new FlightDetailsModel();

        var flightDetailsModelWithoutTicket = new FlightDetailsModel();

        // Act
        var workflowExecution = async () => await worker.ExecuteAsync(async () =>
        {
            var handle = await env.Client.StartWorkflowAsync((FlightWorkflow wf) => wf.Run(flight),
                new WorkflowOptions { Id = flight.Id, TaskQueue = Temporal.DefaultQueue, RetryPolicy = _retryPolicy });

            await env.DelayAsync(TimeSpan.FromMinutes(1));

            await handle.SignalAsync(wf => wf.Book(bookingRequestModel));

            flightDetailsModelWithTicket = await handle.QueryAsync(wf => wf.GetFlightDetails());

            await handle.SignalAsync(wf => wf.BookCompensation(bookingRequestModel));

            flightDetailsModelWithoutTicket = await handle.QueryAsync(wf => wf.GetFlightDetails());

            await handle.GetResultAsync();
        });

        // Assert
        await workflowExecution.Should().NotThrowAsync();

        flightDetailsModelWithTicket.Registered.Should().Contain(bookingRequestModel.Ticket);

        flightDetailsModelWithoutTicket.Registered.Should().NotContain(bookingRequestModel.Ticket);
    }

    [Fact]
    public async Task RegisterPassengerPaymentCancellation_Succeeds()
    {
        // Arrange
        await using var env = await WorkflowEnvironment.StartTimeSkippingAsync();

        var flight = TestFlightFabric.GetTestFlight();

        var ticket = TestTicketFabric.GetTestTicket(flight.Id);

        var bookingRequestModel = new BookingSignalModel(ticket);

        var markTicketPaidRequestModel = new MarkTicketPaidSignalModel(ticket);

        using var worker = await WorkerHelper.ConfigureWorkerAsync(env, _mapper);

        var flightDetailsModelWithPaidTicket = new FlightDetailsModel();

        var flightDetailsModelWithCancelledTicket = new FlightDetailsModel();

        // Act
        var workflowExecution = async () => await worker.ExecuteAsync(async () =>
        {
            var handle = await env.Client.StartWorkflowAsync((FlightWorkflow wf) => wf.Run(flight),
                new WorkflowOptions { Id = flight.Id, TaskQueue = Temporal.DefaultQueue, RetryPolicy = _retryPolicy });

            await env.DelayAsync(TimeSpan.FromMinutes(1));

            await handle.SignalAsync(wf => wf.Book(bookingRequestModel));

            await handle.SignalAsync(wf => wf.MarkTicketPaid(markTicketPaidRequestModel));

            flightDetailsModelWithPaidTicket = await handle.QueryAsync(wf => wf.GetFlightDetails());

            await handle.SignalAsync(wf => wf.MarkTicketPaidCompensation(markTicketPaidRequestModel));

            flightDetailsModelWithCancelledTicket = await handle.QueryAsync(wf => wf.GetFlightDetails());

            await handle.GetResultAsync();
        });

        // Assert
        await workflowExecution.Should().NotThrowAsync();

        flightDetailsModelWithPaidTicket.Registered.Should().Contain(
            t => t.Id == ticket.Id && t.PaymentStatus == PaymentStatus.Paid);
        
        // Assert cancellation
        flightDetailsModelWithCancelledTicket.Registered.Should().Contain(
            t => t.Id == ticket.Id && t.PaymentStatus == PaymentStatus.Cancelled);
    }

    [Fact]
    public async Task CheckInPassenger_Succeeds()
    {
        // Arrange
        await using var env = await WorkflowEnvironment.StartTimeSkippingAsync();

        var flight = TestFlightFabric.GetTestFlight();

        var ticket = TestTicketFabric.GetTestTicket(flight.Id);
        
        var bookingRequestModel = new BookingSignalModel(ticket);

        var seat = flight.Seats.First();

        seat.TicketId = ticket.Id;
        
        var seatReservationRequestModel = new SeatReservationSignalModel(ticket.Id, seat.Name);
            
        using var worker = await WorkerHelper.ConfigureWorkerAsync(env, _mapper);

        var flightDetailsModelWithReservedSeat = new FlightDetailsModel();

        var flightDetailsModelWithEmptySeat = new FlightDetailsModel();

        // Act
        var workflowExecution = async () => await worker.ExecuteAsync(async () =>
        {
            var handle = await env.Client.StartWorkflowAsync((FlightWorkflow wf) => wf.Run(flight),
                new WorkflowOptions { Id = flight.Id, TaskQueue = Temporal.DefaultQueue, RetryPolicy = _retryPolicy });

            await env.DelayAsync(TimeSpan.FromMinutes(1));

            await handle.SignalAsync(wf => wf.Book(bookingRequestModel));
            
            await handle.SignalAsync(wf => wf.ReserveSeat(seatReservationRequestModel));

            flightDetailsModelWithReservedSeat = await handle.QueryAsync(wf => wf.GetFlightDetails());

            await handle.SignalAsync(wf => wf.ReserveSeatCompensation(seatReservationRequestModel));

            flightDetailsModelWithEmptySeat = await handle.QueryAsync(wf => wf.GetFlightDetails());

            await handle.GetResultAsync();
        });

        // Assert
        await workflowExecution.Should().NotThrowAsync();
        
        flightDetailsModelWithReservedSeat.Seats.Should().Contain(
            s => s.Name == seat.Name && s.TicketId == ticket.Id);

        flightDetailsModelWithReservedSeat.Registered.Should().Contain(
            t => t.Id == ticket.Id && t.Seat.Equals(seat));
        
        // Assert cancellation
        flightDetailsModelWithEmptySeat.Seats.Should().Contain(
            s => s.Name == seat.Name && s.TicketId == null);

        flightDetailsModelWithEmptySeat.Registered.Should().Contain(
            t => t.Id == ticket.Id && t.Seat == null);
    }

    [Fact]
    public async Task BoardPassenger_Succeeds()
    {
        // Arrange
        await using var env = await WorkflowEnvironment.StartTimeSkippingAsync();

        var flight = TestFlightFabric.GetTestFlight();
        
        var boardingRequestModel = new BoardingSignalModel(TestTicketFabric.GetTestTicket(flight.Id));

        using var worker = await WorkerHelper.ConfigureWorkerAsync(env, _mapper);

        var flightDetailsModelWithBoardedPassenger = new FlightDetailsModel();

        // Act
        var workflowExecution = async () => await worker.ExecuteAsync(async () =>
        {
            var handle = await env.Client.StartWorkflowAsync((FlightWorkflow wf) => wf.Run(flight),
                new WorkflowOptions { Id = flight.Id, TaskQueue = Temporal.DefaultQueue, RetryPolicy = _retryPolicy });

            await env.DelayAsync(TimeSpan.FromMinutes(1));

            await handle.SignalAsync(wf => wf.BoardPassenger(boardingRequestModel));

            flightDetailsModelWithBoardedPassenger = await handle.QueryAsync(wf => wf.GetFlightDetails());

            await handle.GetResultAsync();
        });

        // Assert
        await workflowExecution.Should().NotThrowAsync();

        flightDetailsModelWithBoardedPassenger.Boarded.Should().Contain(boardingRequestModel.Ticket);
    }
}