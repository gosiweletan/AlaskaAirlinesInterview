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
			return _dataAccess.CreateEvent(newEvent);
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

		public TicketType UpdateTicketType(Guid eventId, TicketType updatedTicketType) {
			return _dataAccess.UpdateTicketType(eventId, updatedTicketType);
		}

		public IEnumerable<Ticket> GetEventTickets(Guid id) {
			return _dataAccess.GetEventTickets(id);
		}

		public TicketType GetEventTicketType(Guid eventId, Guid ticketTypeId) {
			return _dataAccess.GetEventTicketType(eventId, ticketTypeId);
		}
	}
}
