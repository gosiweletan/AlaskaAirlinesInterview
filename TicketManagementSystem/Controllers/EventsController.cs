using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Models;

namespace TicketManagementSystem.Controllers
{
    [ApiController]
    [Route("events")]
    public class EventsController : ControllerBase
    {
        private readonly ILogger<EventsController> _logger;
        private readonly Operations _operations;

        public EventsController(Operations operations, ILogger<EventsController> logger)
        {
            _logger = logger;
            _operations = operations;
        }

		[HttpPost]
		public IActionResult CreateEvent([FromBody] Event newEvent) {
			try {
				return Created("/events/" + newEvent.Id, _operations.CreateEvent(newEvent));
			} 
			catch (Exception ex) {
				return BadRequest("Failed to create event: " + ex.Message);
			}
		}

		[HttpGet]
		[Route("{eventId}")]
		public IActionResult GetEvent([FromRoute] Guid eventId) {
			try {
				var foundEvent = _operations.GetEvent(eventId);
				if (foundEvent == null) {
					return NotFound($"Event with ID {eventId} not found.");
				}

				return Ok(foundEvent);
			}
			catch (Exception ex) {
				return BadRequest("Failed to retrieve event: " + ex.Message);
			}
		}

		[HttpPut]
		[Route("{eventId}")]
		public IActionResult UpdateEvent([FromRoute] Guid eventId, [FromBody] Event updatedEvent) {
			try {
				var updated = _operations.UpdateEvent(eventId, updatedEvent);
				return Ok(updated);
			}
			catch (Exception ex) {
				return BadRequest("Failed to update event: " + ex.Message);
			}
		}
	}
}
