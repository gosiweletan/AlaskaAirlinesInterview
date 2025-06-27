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
				var createdEvent = _operations.CreateEvent(newEvent);
				return Created($"/events/{createdEvent.Id}", createdEvent);
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

		[HttpPost]
		[Route("{eventId}/tickettypes")]
		public IActionResult CreateTicketType([FromRoute] Guid eventId, [FromBody] TicketType newTicketType) {
			try {
				var createdTicketType = _operations.CreateTicketType(eventId, newTicketType);
				return Created($"/events/{eventId}/tickettypes/{createdTicketType.Id}", createdTicketType);
			}
			catch (Exception ex) {
				return BadRequest("Failed to create ticket type: " + ex.Message);
			}
		}

		[HttpGet]
		[Route("{eventId}/tickettypes")]
		public IActionResult GetTicketTypes([FromRoute] Guid eventId) {
			try {
				var ticketTypes = _operations.GetEventTicketTypes(eventId);
				if (ticketTypes == null || !ticketTypes.Any()) {
					return NotFound($"No ticket types found for event {eventId}.");
				}

				return Ok(ticketTypes);
			}
			catch (Exception ex) {
				return BadRequest("Failed to retrieve ticket types: " + ex.Message);
			}
		}

		[HttpGet]
		[Route("{eventId}/tickettypes/{ticketTypeId}")]
		public IActionResult GetTicketType([FromRoute] Guid eventId, [FromRoute] Guid ticketTypeId) {
			try {
				var ticketType = _operations.GetEventTicketType(eventId, ticketTypeId);
				if (ticketType == null) {
					return NotFound($"Ticket type with ID {ticketTypeId} for event {eventId} not found.");
				}

				return Ok(ticketType);
			}
			catch (Exception ex) {
				return BadRequest("Failed to retrieve ticket type: " + ex.Message);
			}
		}

		[HttpPut]
		[Route("{eventId}/tickettypes/{ticketTypeId}")]
		public IActionResult UpdateTicketType([FromRoute] Guid eventId, [FromRoute] Guid ticketTypeId, [FromBody] TicketType updatedTicketType) {
			try {
				var updated = _operations.UpdateTicketType(eventId, ticketTypeId, updatedTicketType);
				return Ok(updated);
			}
			catch (Exception ex) {
				return BadRequest("Failed to update ticket type: " + ex.Message);
			}
		}

		[HttpGet]
		[Route("{eventId}/tickets")]
		public IActionResult GetEventTickets([FromRoute] Guid eventId, [FromQuery] int page, [FromQuery] int pageSize) {
			try {
				var tickets = _operations.GetEventTickets(eventId, page, pageSize);
				return Ok(tickets);
			}
			catch (Exception ex) {
				return BadRequest("Failed to retrieve tickets: " + ex.Message);
			}
		}
	}
}
