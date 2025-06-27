using TicketManagementSystem.Models;
using TicketManagementSystem.DataAccessLayer;
using System.Numerics;

namespace TicketManagementSystem {
	public class Operations {
		private readonly DataAccess _dataAccess = new();
		public int DefaultPageSize { get; set; } = 10;

		public Venue CreateVenue(Venue newVenue) {
			return _dataAccess.CreateVenue(newVenue);
		}

		public IEnumerable<Venue> GetVenues(int pageNumber, int pageSize) {
			return _dataAccess.GetVenues(pageNumber == default ? 1 : pageNumber, pageSize == default ? DefaultPageSize : pageSize);
		}

		public Venue? GetVenue(Guid id) {
			return _dataAccess.GetVenue(id);
		}

		public Venue? UpdateVenue(Guid venueId, Venue updatedVenue) {
			updatedVenue.Id = venueId;
			return _dataAccess.UpdateVenue(updatedVenue);
		}

		public Event CreateEvent(Event newEvent) {
			return _dataAccess.CreateEvent(newEvent);
		}

		public Event? GetEvent(Guid id) {
			return _dataAccess.GetEvent(id);
		}

		public IEnumerable<Event> GetEvents(int pageNumber, int pageSize) {
			return _dataAccess.GetEvents(pageNumber == default ? 1 : pageNumber, pageSize == default ? DefaultPageSize : pageSize);
		}

		public Event UpdateEvent(Guid eventId, Event updatedEvent) {
			updatedEvent.Id = eventId;
			return _dataAccess.UpdateEvent(updatedEvent);
		}

		public TicketType CreateTicketType(Guid eventId, TicketType ticketType) {
			return _dataAccess.CreateTicketType(eventId, ticketType);
		}

		public TicketType UpdateTicketType(Guid eventId, Guid ticketTypeId, TicketType updatedTicketType) {
			updatedTicketType.Id = ticketTypeId;
			return _dataAccess.UpdateTicketType(eventId, updatedTicketType);
		}

		public IEnumerable<Ticket> GetEventTickets(Guid eventId, TicketStatus ticketStatus, int pageNum, int pageSize) {
			if (string.IsNullOrEmpty(ticketStatus.ToString())) {
				ticketStatus = TicketStatus.Unknown;
			}

			return _dataAccess.GetEventTickets(eventId, ticketStatus, pageNum == default ? 1 : pageNum, pageSize == default ? DefaultPageSize : pageSize);
		}

		public Ticket GetEventTicket(Guid eventId, Guid ticketId) {
			return _dataAccess.GetEventTicket(eventId, ticketId);
		}

		public Ticket GetTicket(Guid ticketId) {
			return _dataAccess.GetTicket(ticketId);
		}

		public IEnumerable<TicketType> GetEventTicketTypes(Guid eventId) {
			return _dataAccess.GetEventTicketTypes(eventId);
		}

		public TicketType GetEventTicketType(Guid eventId, Guid ticketTypeId) {
			return _dataAccess.GetEventTicketType(eventId, ticketTypeId);
		}

		public TicketReservation? CreateTicketReservation(Guid ticketId, TicketReservation ticketReservation) {
			return _dataAccess.ReserveTicket(ticketId, ticketReservation);
		}

		public TicketReservation? GetTicketReservation(Guid ticketId, Guid userId) {
			return _dataAccess.GetTicketReservation(ticketId, userId);
		}

		public void DeleteTicketReservation(Guid ticketId, Guid userId) {
			_dataAccess.DeleteTicketReservation(ticketId, userId);
		}

		public TicketPurchase CreateTicketPurchase(Guid ticketId, Guid userId, string purchaseToken, decimal purchasePrice) {
			return _dataAccess.CreateTicketPurchase(ticketId, userId, purchaseToken, purchasePrice);
		}

		public TicketPurchase? GetTicketPurchase(Guid ticketId) {
			return _dataAccess.GetTicketPurchase(ticketId);
		}
	}
}
