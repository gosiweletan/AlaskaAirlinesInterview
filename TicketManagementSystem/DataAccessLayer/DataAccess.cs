using TicketManagementSystem.Models;

namespace TicketManagementSystem.DataAccessLayer {
	public class DataAccess {
		public const string TicketStatusAvailable = "Available";
		public const string TicketStatusReserved = "Reserved";
		public const string TicketStatusSold = "Sold";

		private Dictionary<Guid, Venue> Venues { get; } = [];
		private Dictionary<Guid, Event> Events { get; } = [];
		private Dictionary<Guid, Dictionary<Guid, TicketType>> TicketTypes { get; } = [];
		private Dictionary<Guid, Dictionary<string, Ticket>> Tickets { get; } = [];
		private const int _defaultPageSize = 10;

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
			newEvent.TicketTypeIds = [];
			newEvent.TicketIds = [];
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
			Events[eventId].TicketTypeIds.Add(newTicketType.Id);

			foreach (var seat in newTicketType.Seats) {
				var ticketId = Guid.NewGuid();
				Tickets[eventId].Add(
					seat,
					new Ticket {
						Id = ticketId,
						TicketTypeId = newTicketType.Id,
						EventId = eventId,
						Seat = seat,
						Status = TicketStatusAvailable
					});
				Events[eventId].TicketIds.Add(ticketId);
			}

			return newTicketType;
		}

		internal TicketType UpdateTicketType(Guid eventId, TicketType updatedTicketType) {
			if (!TicketTypes.ContainsKey(eventId)) {
				throw new InvalidOperationException($"EventId {eventId} not found.");
			}

			if (!TicketTypes[eventId].ContainsKey(updatedTicketType.Id)) {
				throw new InvalidOperationException($"Ticket type id {updatedTicketType.Id} not found.");
			}

			var newSeats = updatedTicketType.Seats?.OrderBy(s => s).ToArray() ?? Array.Empty<string>();
			var oldSeats = TicketTypes[eventId][updatedTicketType.Id].Seats?.OrderBy(s => s).ToArray() ?? Array.Empty<string>();
			var addedTickets = new List<Ticket>();
			var deletedTickets = new List<Ticket>();
			for (int newIx = 0, oldIx = 0; newIx < newSeats.Length && oldIx < oldSeats.Length;) {
				if (newIx < newSeats.Length && oldIx < oldSeats.Length && newSeats[newIx] == oldSeats[oldIx]) {
					newIx++;
					oldIx++;
				}
				else if (newIx < newSeats.Length && string.Compare(newSeats[newIx], oldSeats[oldIx], StringComparison.Ordinal) < 0) {
					var newTicketId = Guid.NewGuid();
					addedTickets.Add(new Ticket {
							Id = newTicketId,
							TicketTypeId = updatedTicketType.Id,
							EventId = eventId,
							Seat = newSeats[newIx],
							Status = TicketStatusAvailable
					});					
					newIx++;
				}
				else {
					string oldSeat = oldSeats[oldIx];
					if (Tickets[eventId][oldSeat].Status != TicketStatusAvailable) {
						throw new InvalidOperationException($"Cannot remove seat {oldSeat} because its ticket is \"{Tickets[eventId][oldSeat].Status}\".");
					}

					deletedTickets.Add(Tickets[eventId][oldSeat]);
					oldIx++;
				}
			}

			// Cannot add / remove tickets inline above because there might be errors that could leave the data in an inconsistent state.
			foreach (Ticket ticket in addedTickets) {
				Tickets[eventId].Add(ticket.Seat, ticket);
				Events[eventId].TicketIds.Add(ticket.Id);
			}

			foreach (Ticket ticket in deletedTickets) {
				Tickets[eventId].Remove(ticket.Seat);
				Events[eventId].TicketIds.Remove(ticket.Id);
			}

			TicketTypes[eventId][updatedTicketType.Id] = updatedTicketType;
			return updatedTicketType;
		}

		internal IEnumerable<Ticket> GetEventTickets(Guid id, int pageNumber = 1, int pageSize = _defaultPageSize) {
			if (!Tickets.ContainsKey(id)) {
				return [];
			}

			if (pageSize <= 0) {
				throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");
			}

			if (pageNumber <= 0) {
				throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than zero.");
			}

			var tickets = Tickets[id];
			var totalTickets = tickets.Count;
			var totalPages = (int)Math.Ceiling((double)totalTickets / pageSize);
			if (pageNumber > totalPages) {
				throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number is out of range.");
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
	}
}
