using TicketManagementSystem;
using TicketManagementSystem.Models;

namespace TicketManagementTests {
	public class TestBase {
		protected Operations _operations = new();
		protected Guid _venueId;

		/// <summary>
		/// Initializes the test environment by creating a venue.
		/// </summary>
		protected void CreateOperationsAndVenue() {
			_operations = new Operations();
			var newVenue = new Venue { Name = "MiniStadium", Seats = ["A1", "A2", "B1", "B2"] };
			var createdVenue = _operations.CreateVenue(newVenue);
			_venueId = createdVenue.Id;
		}

		protected Event CreateEvent(bool addTicketType = false) {
			var newEvent = GenerateEvent();
			var createdEvent = _operations.CreateEvent(newEvent);
			if (addTicketType) {
				var ticketType = GenerateVipTicketType();
				_operations.CreateTicketType(createdEvent.Id, ticketType);
			}

			return createdEvent;
		}

		protected Event GenerateEvent() {
			return new Event {
				Name = "Singalot Everett",
				VenueId = _venueId,
				EventStart = DateTime.UtcNow.AddDays(10),
				EventEnd = DateTime.UtcNow.AddDays(11),
				ForSaleStart = DateTime.UtcNow,
				ForSaleEnd = DateTime.UtcNow.AddDays(11),
				Description = "A concert by Singalot in Everett"
			};
		}

		protected TicketType GenerateVipTicketType() {
			return new TicketType {
				Name = "VIP",
				Price = 100.00m,
				Seats = ["A1", "A2"]
			};
		}
	}
}
