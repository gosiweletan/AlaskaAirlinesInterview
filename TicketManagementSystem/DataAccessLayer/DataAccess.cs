using TicketManagementSystem.Models;

namespace TicketManagementSystem.DataAccessLayer {
	public class DataAccess {
		private Dictionary<Guid, Venue> Venues { get; } = [];
		private Dictionary<Guid, Event> Events { get; } = [];

		/// <summary> Hierarchy is: EventId --> TicketTypeId --> TicketType --> Seats </summary>
		private Dictionary<Guid, Dictionary<Guid, TicketType>> TicketTypes { get; } = [];

		/// <summary> Hierarchy is: EventId --> TicketId --> Ticket </summary>
		private Dictionary<Guid, Dictionary<Guid, Ticket>> Tickets { get; } = [];

		private const int MaxPageSize = 1000;

		public Venue CreateVenue(Venue newVenue) {
			newVenue.Id = Guid.NewGuid();
			Venues.Add(newVenue.Id, newVenue);
			return newVenue;
		}

		public Venue GetVenue(Guid id) {
			return Venues[id];
		}

		public Event CreateEvent(Event newEvent) {
			newEvent.Id = Guid.NewGuid();
			Events.Add(newEvent.Id, newEvent);
			TicketTypes[newEvent.Id] = [];
			Tickets[newEvent.Id] = [];
			return newEvent;
		}

		internal Event UpdateEvent(Event updatedEvent) {
			if (!Events.ContainsKey(updatedEvent.Id)) {
				throw new InvalidOperationException($"Event with ID {updatedEvent.Id} not found.");
			}

			Events[updatedEvent.Id] = updatedEvent;
			return updatedEvent;
		}

		internal Event? GetEvent(Guid id) {
			return Events.TryGetValue(id, out var ev) ? ev : null;
		}

		internal TicketType CreateTicketType(Guid eventId, TicketType newTicketType) {
			if (!Events.ContainsKey(eventId)) {
				throw new InvalidOperationException($"EventId {eventId} not found.");
			}

			newTicketType.Id = Guid.NewGuid();
			TicketTypes[eventId].Add(newTicketType.Id, newTicketType);
			CreateEventTicketsForSeats(eventId, newTicketType);

			return newTicketType;
		}

		internal TicketType UpdateTicketType(Guid eventId, TicketType updatedTicketType) {
			if (!TicketTypes.ContainsKey(eventId)) {
				throw new InvalidOperationException($"EventId {eventId} not found.");
			}

			if (!TicketTypes[eventId].ContainsKey(updatedTicketType.Id)) {
				throw new InvalidOperationException($"Ticket type id {updatedTicketType.Id} not found.");
			}

			var ticketsForType = Tickets[eventId].Values.Where(ticket => ticket.TicketTypeId == updatedTicketType.Id).ToList();
			if (!updatedTicketType.Seats.SequenceEqual(ticketsForType.Select(t => t.Seat)) && ticketsForType.Any(ticket => ticket.Status != TicketStatus.Available)) {
				throw new InvalidOperationException("Cannot update seats for ticket type because tickets are already being sold.");
			}

			// While it is less efficient to remove and recreate all tickets, this happens rarely and the code is cheaper to maintain.
			foreach (var ticket in ticketsForType) {
				Tickets[eventId].Remove(ticket.Id);
			}

			CreateEventTicketsForSeats(eventId, updatedTicketType);

			TicketTypes[eventId][updatedTicketType.Id] = updatedTicketType;
			return updatedTicketType;
		}

		private void CreateEventTicketsForSeats(Guid eventId, TicketType newTicketType) {
			foreach (var seat in newTicketType.Seats) {
				var ticketId = Guid.NewGuid();
				Tickets[eventId].Add(
					ticketId,
					new Ticket {
						Id = ticketId,
						TicketTypeId = newTicketType.Id,
						EventId = eventId,
						Seat = seat
					});
			}
		}

		internal IEnumerable<Ticket> GetEventTickets(Guid eventId, int pageNumber, int pageSize) {
			if (pageSize <= 0) {
				throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
			}

			if (pageSize > MaxPageSize) {
				throw new ArgumentOutOfRangeException(nameof(pageSize), $"Page size cannot exceed {MaxPageSize}.");
			}

			if (pageNumber <= 0) {
				throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
			}

			var tickets = Tickets[eventId];
			var totalTickets = tickets.Count;
			var totalPages = (int)Math.Ceiling((double)totalTickets / pageSize);
			if (pageNumber > totalPages) {
				throw new ArgumentOutOfRangeException(nameof(pageNumber), $"Page number is out of range. {totalPages} pages of size {pageSize} exist.");
			}

			return tickets.Values.Skip((pageNumber - 1) * pageSize).Take(pageSize);
		}

		internal TicketType GetEventTicketType(Guid eventId, Guid ticketTypeId) {
			if (!TicketTypes.ContainsKey(eventId)) {
				throw new InvalidOperationException($"Event id {eventId} not found.");
			}

			if (!TicketTypes[eventId].ContainsKey(ticketTypeId)) {
				throw new InvalidOperationException($"Ticket type id {ticketTypeId} not found for event id {eventId}.");
			}

			return TicketTypes[eventId][ticketTypeId];
		}

		internal IEnumerable<TicketType> GetEventTicketTypes(Guid eventId) {
			return TicketTypes.ContainsKey(eventId) ? TicketTypes[eventId].Values : [];
		}

		internal Ticket GetEventTicket(Guid eventId, Guid ticketId) {
			if (!Tickets.TryGetValue(eventId, out Dictionary<Guid, Ticket>? eventTickets)) {
				throw new InvalidOperationException($"Event id {eventId} not found.");
			}

			if (!eventTickets.TryGetValue(ticketId, out Ticket? ticket)) {
				throw new InvalidOperationException($"Ticket id {ticketId} not found for event id {eventId}.");
			}

			return ticket;
		}

		internal Ticket GetTicket(Guid ticketId) {
			var ticket = Tickets.Values.SelectMany(t => t.Values).FirstOrDefault(t => t.Id == ticketId);
			if (ticket == null) {
				throw new InvalidOperationException($"Ticket id {ticketId} not found.");
			}

			return ticket;
		}

		internal TicketReservation ReserveTicket(Guid ticketId, TicketReservation ticketReservation) {
			if (ticketReservation.UserId == Guid.Empty) {
				throw new ArgumentException("UserId is required.", nameof(ticketReservation));
			}

			var ticket = GetTicket(ticketId);
			if (ticket.Status != TicketStatus.Available) {
				throw new InvalidOperationException($"Ticket id {ticketId} is not available for reservation because its status is already {ticket.Status}.");
			}

			ticket.Owner = ticketReservation.UserId;
			ticket.ReservedUntil = DateTime.UtcNow.AddMinutes(10);
			ticketReservation.ReservedUntil = ticket.ReservedUntil.Value;
			ticketReservation.Id = ticketReservation.UserId;
			return ticketReservation;
		}

		/// <summary>
		/// Ensures that a ticket is not reserved for a user.
		/// Idempotent. Will have no effect if the ticket is not reserved or has been reserved by a different user.
		/// </summary>
		internal void DeleteTicketReservation(Guid ticketId, Guid userId) {
			var ticket = GetTicket(ticketId);
			if (ticket.Status == TicketStatus.Reserved && ticket.Owner == userId) {
				// note: do not delete the owner to avoid a race condition where the ticket gets purchased as the cancellation call is being executed
				ticket.ReservedUntil = null;
			}
		}

		internal TicketReservation? GetTicketReservation(Guid ticketId, Guid userId) {
			var ticket = GetTicket(ticketId);
			if (ticket.Status != TicketStatus.Reserved || ticket.Owner != userId) {
				// No reservation found.
				return null;
			}

			return new TicketReservation {
				Id = userId,
				UserId = userId,
				ReservedUntil = ticket.ReservedUntil.Value
			};
		}

		internal TicketPurchase CreateTicketPurchase(Guid ticketId, Guid purchaser, string purchaseToken, decimal purchasePrice) {
			var ticket = GetTicket(ticketId);

			// If the user has already purchased the ticket, behave idempotently. Deliberately ignoring the purchase token and price since the important fact is that the ticket is purchased.
			if (ticket.Status == TicketStatus.Purchased && ticket.Owner == purchaser) {
				return new TicketPurchase {
					UserId = purchaser,
					PurchaseToken = ticket.PurchaseToken,
					PurchasePrice = ticket.PurchasePrice.Value
				};
			}

			if (ticket.Status == TicketStatus.Purchased) {
				throw new InvalidOperationException($"Ticket id {ticketId} is not available for purchase because it has already been purchased by someone else.");
			}

			if (ticket.Status == TicketStatus.Reserved && ticket.Owner != purchaser) {
				throw new InvalidOperationException($"Ticket id {ticketId} is reserved by another user and cannot be purchased.");
			}

			var ticketType = TicketTypes[ticket.EventId][ticket.TicketTypeId];
			if (purchasePrice != ticketType.Price) {
				throw new InvalidOperationException($"Ticket id {ticketId} cannot be purchased for {purchasePrice:C} because the actual price is {ticketType.Price:C}.");
			}

			// TODO: Use payment processing system to process the payment.

			ticket.PurchaseToken = purchaseToken;
			ticket.PurchasePrice = purchasePrice;

			return new TicketPurchase {
				UserId = purchaser,
				PurchaseToken = purchaseToken,
				PurchasePrice = purchasePrice
			};
		}

		internal TicketPurchase? GetTicketPurchase(Guid ticketId) {
			var ticket = GetTicket(ticketId);
			if (ticket.Status != TicketStatus.Purchased) {
				// No purchase found.
				return null;
			}

			return new TicketPurchase {
				UserId = ticket.Owner.Value,
				PurchaseToken = ticket.PurchaseToken,
				PurchasePrice = ticket.PurchasePrice.Value
			};
		}
	}
}
