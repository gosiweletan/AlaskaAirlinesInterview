using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TicketManagementSystem.Models;

namespace TicketManagementSystem.Controllers
{
    [ApiController]
    [Route("Tickets")]
    public class TicketsController : ControllerBase
    {
        private readonly ILogger<TicketsController> _logger;
        private readonly Operations _operations;

        public TicketsController(Operations operations, ILogger<TicketsController> logger)
        {
            _logger = logger;
            _operations = operations;
        }

		[HttpGet]
		[Route("{ticketId}")]
		public IActionResult GetTicket([FromRoute] Guid ticketId) {
			try {
				var foundTicket = _operations.GetTicket(ticketId);
				if (foundTicket == null) {
					return NotFound($"Ticket with ID {ticketId} not found.");
				}

				return Ok(foundTicket);
			}
			catch (Exception ex) {
				return BadRequest("Failed to retrieve ticket: " + ex.Message);
			}
		}

		[HttpPost]
		[Route("{ticketId}/reservations")]
		public IActionResult CreateTicketReservation([FromRoute] Guid ticketId, [FromBody] TicketReservation newReservation) {
			try {
				var createdReservation = _operations.CreateTicketReservation(ticketId, newReservation);
				if (createdReservation == null) {
					return NotFound($"Ticket with ID {ticketId} not found.");
				}

				return Created($"/tickets/{ticketId}/reservations/{createdReservation.UserId}", createdReservation);
			} 
			catch (Exception ex) {
				return BadRequest("Failed to create ticket: " + ex.Message);
			}
		}

		[HttpGet]
		[Route("{ticketId}/reservations/{userId}")]
		public IActionResult GetTicketReservation([FromRoute] Guid ticketId, [FromRoute] Guid userId) {
			try {
				var reservation = _operations.GetTicketReservation(ticketId, userId);
				if (reservation == null) {
					return NotFound($"Reservation for User {userId} on Ticket {ticketId} not found.");
				}

				return Ok(reservation);
			}
			catch (Exception ex) {
				return BadRequest("Failed to retrieve ticket reservation: " + ex.Message);
			}
		}

		[HttpDelete]
		[Route("{ticketId}/reservations/{userId}")]
		public IActionResult DeleteTicketReservation([FromRoute] Guid ticketId, [FromRoute] Guid userId) {
			try {
				_operations.DeleteTicketReservation(ticketId, userId);
				return NoContent();
			}
			catch (Exception ex) {
				return BadRequest("Failed to delete ticket reservation: " + ex.Message);
			}
		}

		[HttpPost]
		[Route("{ticketId}/purchase")]
		public IActionResult CreateTicketPurchase([FromRoute] Guid ticketId, [FromBody] TicketPurchase newPurchase) {
			try {
				var createdPurchase = _operations.CreateTicketPurchase(ticketId, newPurchase.PurchaserId, newPurchase.PurchaseToken, newPurchase.PurchasePrice);
				return Created($"/tickets/{ticketId}/purchase", createdPurchase);
			}
			catch (Exception ex) {
				return BadRequest("Failed to create ticket purchase: " + ex.Message);
			}
		}

		[HttpGet]
		[Route("{ticketId}/purchase")]
		public IActionResult GetTicketPurchase([FromRoute] Guid ticketId) {
			try {
				var purchase = _operations.GetTicketPurchase(ticketId);
				if (purchase == null) {
					return NotFound($"Purchase for Ticket {ticketId} not found.");
				}

				return Ok(purchase);
			}
			catch (Exception ex) {
				return BadRequest("Failed to retrieve ticket purchase: " + ex.Message);
			}
		}
	}
}
