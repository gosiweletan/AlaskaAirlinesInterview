using TicketManagementSystem.Models;
using TicketManagementSystem.DataAccessLayer;

namespace TicketManagementSystem {
	public class Operations {
		private readonly DataAccess _dataAccess = new();

		public int DefaultPageSize { get; set; } = 10;


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

		public TicketType CreateTicketType(Guid eventId, TicketType ticketType) {
			return _dataAccess.CreateTicketType(eventId, ticketType);
		}

		public TicketType UpdateTicketType(Guid eventId, Guid ticketTypeId, TicketType updatedTicketType) {
			updatedTicketType.Id = ticketTypeId;
			return _dataAccess.UpdateTicketType(eventId, updatedTicketType);
		}

		public IEnumerable<Ticket> GetEventTickets(Guid eventId) {
			return _dataAccess.GetEventTickets(eventId, 1, DefaultPageSize);
		}

		public TicketType GetEventTicketType(Guid eventId, Guid ticketTypeId) {
			return _dataAccess.GetEventTicketType(eventId, ticketTypeId);
		}

		public IEnumerable<TicketType> GetEventTicketTypes(Guid eventId) {
			return _dataAccess.GetEventTicketTypes(eventId);
		}
	}
}
