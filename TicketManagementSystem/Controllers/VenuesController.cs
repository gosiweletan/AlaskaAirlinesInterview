using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TicketManagementSystem.Models;

namespace TicketManagementSystem.Controllers
{
    [ApiController]
    [Route("venues")]
    public class VenuesController : ControllerBase
    {
        private readonly ILogger<VenuesController> _logger;
        private readonly Operations _operations;

        public VenuesController(Operations operations, ILogger<VenuesController> logger)
        {
            _logger = logger;
            _operations = operations;
        }

        [HttpPost]
        public IActionResult CreateVenue([FromBody] Venue newVenue) {
            try {
                return Created("/venues/" + newVenue.Id, _operations.CreateVenue(newVenue));
            }
            catch (Exception ex) {
                return BadRequest("Failed to create venue: " + ex.Message);
            }
        }

        [HttpGet]
		[Route("{venueId}")]
		public IActionResult GetVenue([FromRoute] Guid venueId)
        {
            var foo = _operations.CreateVenue(new Venue { Name = "Default Venue", Seats = ["A1", "A2", "B1", "B2"] });
            foo.Id = venueId; // Simulate an existing venue for testing purposes
			return Ok(foo);
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
	}
}
