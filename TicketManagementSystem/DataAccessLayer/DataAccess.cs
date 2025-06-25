using TicketManagementSystem.Models;

namespace TicketManagementSystem.DataAccessLayer {
	public class DataAccess {
		private Dictionary<Guid, Venue> Venues = [];
		public Venue CreateVenue(Venue newVenue) {
			newVenue.Id = Guid.NewGuid();
			Venues.Add(newVenue.Id, newVenue);
			return newVenue;
		}

		internal Venue GetVenue(Guid id) {
			return Venues[id];
		}
	}
}
