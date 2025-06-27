using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TicketManagementSystem.Models;

namespace TicketManagementSystem.Controllers
{
    [ApiController]
    [Route("venues")]
    public class VenuesController : ControllerBase {
        private readonly ILogger<VenuesController> _logger;
        private readonly Operations _operations;

        public VenuesController(Operations operations, ILogger<VenuesController> logger) {
            _logger = logger;
            _operations = operations;
        }

        [HttpPost]
        public IActionResult CreateVenue([FromBody] Venue newVenue) {
            try {
                var createdVenue = _operations.CreateVenue(newVenue);
                return Created("/venues/" + createdVenue.Id, createdVenue);
            }
            catch (Exception ex) {
                return BadRequest("Failed to create venue: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetVenues([FromQuery] int page, [FromQuery] int pageSize) {
            try {
                var venues = _operations.GetVenues(page, pageSize);
                return Ok(venues);
            }
            catch (Exception ex) {
                return BadRequest("Failed to retrieve venues: " + ex.Message);
            }
        }

        [HttpGet]
        [Route("{venueId}")]
        public IActionResult GetVenue([FromRoute] Guid venueId) {
            try {
                var foundVenue = _operations.GetVenue(venueId);
                if (foundVenue == null) {
                    return NotFound($"Event with ID {venueId} not found.");
                }

                return Ok(foundVenue);
            }
            catch (Exception ex) {
                return BadRequest("Failed to retrieve event: " + ex.Message);
            }
        }

        [HttpPut]
        [Route("{venueId}")]
        public IActionResult UpdateVenue([FromRoute] Guid venueId, [FromBody] Venue updatedVenue) {
            try {
                updatedVenue.Id = venueId;
                var updated = _operations.UpdateVenue(venueId, updatedVenue);
                if (updated == null) {
                    return NotFound($"Venue with ID {venueId} not found.");
                }
                return Ok(updated);
            }
            catch (Exception ex) {
                return BadRequest("Failed to update venue: " + ex.Message);
            }
        }
    }
}
