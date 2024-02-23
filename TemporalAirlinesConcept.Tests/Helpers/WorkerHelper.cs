using AutoMapper;
using Moq;
using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.DAL.Entities;
using TemporalAirlinesConcept.DAL.Implementations;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using Temporalio.Testing;
using Temporalio.Worker;

namespace TemporalAirlinesConcept.Tests.Helpers;

public static class WorkerHelper
{
    public static async Task<TemporalWorker> ConfigureWorkerAsync(WorkflowEnvironment env,
        IMapper mapper,
        IMock<IRepository<Flight>> flightMockRepository = null,
        IMock<IRepository<Ticket>> ticketMockRepository = null)
    {
        flightMockRepository ??= new Mock<IRepository<Flight>>();
        ticketMockRepository ??= new Mock<IRepository<Ticket>>();

        var mockUnitOfWork = new Mock<UnitOfWork>();

        mockUnitOfWork
            .Setup(x => x.Repository<Flight>())
            .Returns(flightMockRepository.Object);

        mockUnitOfWork
          .Setup(x => x.Repository<Ticket>())
          .Returns(ticketMockRepository.Object);

        var flightActivities = new FlightActivities(mockUnitOfWork.Object, mapper);

        return new TemporalWorker(env.Client,
            new TemporalWorkerOptions(Temporal.DefaultQueue)
                .AddWorkflow<FlightWorkflow>()
                .AddAllActivities(flightActivities));
    }
}