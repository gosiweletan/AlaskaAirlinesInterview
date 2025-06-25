using TicketManagementSystem.Models;

namespace TicketManagementSystem.DataAccessLayer {
	public class DataAccess {
		private Dictionary<Guid, Venue> Venues = [];
		private Dictionary<Guid, Event> Events = [];

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

		internal Event? GetEvent(Guid id) {
			return Events.TryGetValue(id, out var ev) ? ev : null;
		}
	}
}
