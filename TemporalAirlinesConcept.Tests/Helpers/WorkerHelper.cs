using AutoMapper;
using Moq;
using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.DAL.Implementations;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Implementations.Flight;
using Temporalio.Testing;
using Temporalio.Worker;

namespace TemporalAirlinesConcept.Tests.Helpers;

public static class WorkerHelper
{
    public static async Task<TemporalWorker> ConfigureWorkerAsync(WorkflowEnvironment env,
        IMapper mapper, IMock<IFlightRepository>? mockFlightRepository = null)
    {
        mockFlightRepository ??= new Mock<IFlightRepository>();

        var mockUnitOfWork = new Mock<UnitOfWork>(
            mockFlightRepository.Object,
            new Mock<ITicketRepository>().Object,
            new Mock<IUserRepository>().Object
        );

        var flightActivities = new FlightActivities(mockUnitOfWork.Object, mapper);

        return new TemporalWorker(env.Client,
            new TemporalWorkerOptions(Temporal.DefaultQueue)
                .AddWorkflow<FlightWorkflow>()
                .AddAllActivities(flightActivities));
    }
}