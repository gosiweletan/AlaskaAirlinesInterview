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
	}
}
