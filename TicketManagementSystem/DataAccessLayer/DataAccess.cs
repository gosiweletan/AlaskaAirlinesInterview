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
			if (!updatedTicketType.Seats.SequenceEqual(ticketsForType.Select(t => t.Seat)) && ticketsForType.Any(ticket => ticket.Status != Ticket.TicketStatus.Available)) {
				throw new InvalidOperationException("Cannot update seats for ticket type because tickets are already being sold.");
			}

			// While it is less efficient to remove and recreate all tickets, this happens rarely and the code is much simpler to maintain.
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
	}
}
