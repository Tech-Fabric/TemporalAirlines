using AutoMapper;
using TemporalAirlinesConcept.Common.Constants;
using TemporalAirlinesConcept.Common.Exceptions;
using TemporalAirlinesConcept.Common.Extensions;
using TemporalAirlinesConcept.DAL.Interfaces;
using TemporalAirlinesConcept.Services.Interfaces.Flight;
using TemporalAirlinesConcept.Services.Models.Flight;
using Temporalio.Client;

namespace TemporalAirlinesConcept.Services.Implementations.Flight;

public class FlightService : IFlightService
{
    private readonly IMapper _mapper;
    private readonly ITemporalClient _temporalClient;
    private readonly IUnitOfWork _unitOfWork;

    public FlightService(IMapper mapper, ITemporalClient temporalClient, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _temporalClient = temporalClient;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DAL.Entities.Flight>> GetFlights()
    {
        var flights = (await _unitOfWork.Repository<DAL.Entities.Flight>()
            .GetAll())
            .ToList();

        var flightDetails = await Task.WhenAll(flights.Select(FetchFlightDetailFromWorkflow));

        flights = flightDetails.ToList();

        return flights;
    }

    public async Task<DAL.Entities.Flight> GetFlight(Guid id)
    {
        var flight = await _unitOfWork.Repository<DAL.Entities.Flight>()
            .FindAsync(x => x.Id == id);

        var fetchedFlight = await FetchFlightDetailFromWorkflow(flight);

        return fetchedFlight;
    }

    public async Task<DAL.Entities.Flight> FetchFlightDetailFromWorkflow(DAL.Entities.Flight flight)
    {
        if (flight is null)
            throw new EntityNotFoundException("Flight was not found.");

        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(flight.Id.ToString()))
            return flight;

        var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flight.Id.ToString());

        var flightDetails = await handle.QueryAsync(wf => wf.GetFlightDetails());

        return _mapper.Map<DAL.Entities.Flight>(flightDetails);
    }

    public async Task<DAL.Entities.Flight> CreateFlight(FlightInputModel model)
    {
        var flight = _mapper.Map<DAL.Entities.Flight>(model);

        _unitOfWork.Repository<DAL.Entities.Flight>().Insert(flight);

        await _unitOfWork.SaveChangesAsync();

        await _temporalClient.StartWorkflowAsync((FlightWorkflow wf) => wf.Run(flight),
            new WorkflowOptions(flight.Id.ToString(), Temporal.DefaultQueue));

        return flight;
    }

    public async Task RemoveFlight(Guid id)
    {
        var flight = await _unitOfWork.Repository<DAL.Entities.Flight>()
            .FindAsync(x => x.Id == id);

        if (flight is null)
            throw new EntityNotFoundException("Flight is not found.");

        _unitOfWork.Repository<DAL.Entities.Flight>().Remove(flight);

        if (!await _temporalClient.IsWorkflowRunning<FlightWorkflow>(flight.Id.ToString()))
            return;

        var handle = _temporalClient.GetWorkflowHandle<FlightWorkflow>(flight.Id.ToString());

        await handle.CancelAsync();
    }
}