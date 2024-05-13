using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Api.Models.Flights;
using TemporalAirlinesConcept.Services.Interfaces.Flight;
using TemporalAirlinesConcept.Services.Models.Flight;

namespace TemporalAirlinesConcept.Api.Controllers
{
    [Route("api/flights")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;
        private readonly IMapper _mapper;

        public FlightsController(IFlightService flightService, IMapper mapper)
        {
            _flightService = flightService;
            _mapper = mapper;
        }

        // GET: api/flights
        [HttpGet]
        public async Task<ActionResult<List<FlightResponse>>> GetFlights()
        {
            var flights = await _flightService.GetFlights();
            var flightsResponse = _mapper.Map<List<FlightResponse>>(flights);

            return Ok(flightsResponse);
        }

        // GET: api/flights/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<FlightResponse>> GetFlight(Guid id)
        {
            var flight = await _flightService.GetFlight(id);
            var flightResponse = _mapper.Map<FlightResponse>(flight);

            return Ok(flightResponse);
        }

        // POST: api/flights/{id}
        [HttpPost]
        public async Task<ActionResult<FlightResponse>> CreateFlight(FlightInputModel model)
        {
            var createdFlight = await _flightService.CreateFlight(model);
            var createdFlightResponse = _mapper.Map<FlightResponse>(createdFlight);

            return CreatedAtAction(nameof(GetFlight), new { id = createdFlight.Id }, createdFlightResponse);
        }

        // DELETE: api/flights/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlight(Guid id)
        {
            await _flightService.RemoveFlight(id);

            return NoContent();
        }
    }
}
