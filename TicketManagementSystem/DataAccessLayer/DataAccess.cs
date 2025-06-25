using TicketManagementSystem.Models;

namespace TicketManagementSystem.DataAccessLayer {
	public class DataAccess {
		private readonly Dictionary<Guid, Venue> Venues = [];
		private readonly Dictionary<Guid, Event> Events = [];
		private readonly Dictionary<Guid, Dictionary<Guid, TicketType>> TicketTypes = [];

		public Event AddEvent(Event newEvent) {
			newEvent.Id = Guid.NewGuid();
			Events.Add(newEvent.Id, newEvent);
			return newEvent;
		}

		public Venue CreateVenue(Venue newVenue) {
			newVenue.Id = Guid.NewGuid();
			Venues.Add(newVenue.Id, newVenue);
			return newVenue;
		}

		public Venue GetVenue(Guid id) {
			return Venues[id];
		}

		internal TicketType CreateTicketType(Guid eventId, TicketType newTicketType) {
			newTicketType.Id = Guid.NewGuid();
			if (!TicketTypes.ContainsKey(eventId)) {
				TicketTypes[eventId] = new Dictionary<Guid, TicketType>();
			}

			TicketTypes[eventId].Add(newTicketType.Id, newTicketType);

			if (Events[eventId].TicketTypeIds == null) {
				Events[eventId].TicketTypeIds = new List<Guid>();
			}

			Events[eventId].TicketTypeIds.Add(newTicketType.Id);
			return newTicketType;
		}

		internal Event? GetEvent(Guid id) {
			return Events.TryGetValue(id, out var ev) ? ev : null;
		}

		internal Event UpdateEvent(Event updatedEvent) {
			Events[updatedEvent.Id] = updatedEvent;
			return updatedEvent;
		}
	}
}
