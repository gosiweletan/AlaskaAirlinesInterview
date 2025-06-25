using TicketManagementSystem;
using TicketManagementSystem.Models;

namespace TicketManagementTests {
	[TestClass]
	public sealed class EventTests {
		private static readonly Operations Operations = new();
		private static Guid VenueId;

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			var newVenue = new Venue { Name = "MiniStadium", Seats = ["A1", "A2", "B1", "B2"] };
			var createdVenue = Operations.CreateVenue(newVenue);
			VenueId = createdVenue.Id;
		}

		[TestMethod]
		public void BasicCreate() {
			var newEvent = new Event {
				Name = "Singalot Everett",
				VenueId = VenueId,
				EventStart = DateTime.UtcNow.AddDays(10),
				EventEnd = DateTime.UtcNow.AddDays(11),
				ForSaleStart = DateTime.UtcNow,
				ForSaleEnd = DateTime.UtcNow.AddDays(11),
				Description = "A concert by Singalot Everett"
			};

			var createdEvent = Operations.CreateEvent(newEvent);
			var defaultGuid = new Guid();
			Assert.IsTrue(createdEvent.Id != defaultGuid);
			var retrievedEvent = Operations.GetEvent(createdEvent.Id);
			Assert.AreEqual(createdEvent, retrievedEvent);
		}

		[TestMethod]
		public void NotFound() {
			var notFoundEvent = Operations.GetEvent(Guid.NewGuid());
			Assert.IsNull(notFoundEvent);
		}
	}
}
