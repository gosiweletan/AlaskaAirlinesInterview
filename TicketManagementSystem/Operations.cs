using TicketManagementSystem.Models;
using TicketManagementSystem.DataAccessLayer;

namespace TicketManagementSystem {
	public class Operations {
		private readonly DataAccess _dataAccess = new();

		public Venue CreateVenue(Venue newVenue) {
			return _dataAccess.CreateVenue(newVenue);
		}

		public Venue GetVenue(Guid id) {
			return _dataAccess.GetVenue(id);
		}

		public Event CreateEvent(Event newEvent) {
			return _dataAccess.AddEvent(newEvent);
		}

		public Event? GetEvent(Guid id) {
			return _dataAccess.GetEvent(id);
		}

		public Event UpdateEvent(Event updatedEvent) {
			return _dataAccess.UpdateEvent(updatedEvent);
		}

		public TicketType CreateTicketType(Guid id, TicketType ticketType) {
			return _dataAccess.CreateTicketType(id, ticketType);
		}
	}
}
