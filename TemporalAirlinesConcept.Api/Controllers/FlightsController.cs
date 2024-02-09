using Microsoft.AspNetCore.Mvc;
using TemporalAirlinesConcept.Services.Interfaces.Flight;
using TemporalAirlinesConcept.Services.Models.Flight;

namespace TemporalAirlinesConcept.Api.Controllers
{
    [Route("api/flights")]
    [ApiController]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;

        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        // GET: api/flights
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DAL.Entities.Flight>>> GetFlights()
        {
            var flights = await _flightService.GetFlights();

            return Ok(flights);
        }

        // GET: api/flights/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DAL.Entities.Flight>> GetFlight(string id)
        {
            var flight = await _flightService.GetFlight(id);

            return Ok(flight);
        }

        // POST: api/flights/{id}
        [HttpPost]
        public async Task<ActionResult<DAL.Entities.Flight>> CreateFlight(FlightInputModel model)
        {
            var createdFlight = await _flightService.CreateFlight(model);

            return CreatedAtAction(nameof(GetFlight), new { id = createdFlight.Id }, createdFlight);
        }

        // DELETE: api/flights/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFlight(string id)
        {
            await _flightService.RemoveFlight(id);

            return NoContent();
        }
    }
}
