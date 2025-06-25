using TicketManagementSystem;
using TicketManagementSystem.Models;

namespace TicketManagementTests {
	[TestClass]
	public sealed class EventTests {
		private Operations _operations = new();
		private Guid VenueId;

		[TestInitialize]
		public void TestInitialize() {
			_operations = new Operations();
			var newVenue = new Venue { Name = "MiniStadium", Seats = ["A1", "A2", "B1", "B2"] };
			var createdVenue = _operations.CreateVenue(newVenue);
			VenueId = createdVenue.Id;
		}

		[TestMethod]
		public void BasicCreate() {
			var createdEvent = CreateEvent();
			var defaultGuid = new Guid();
			Assert.IsTrue(createdEvent.Id != defaultGuid);
			var retrievedEvent = _operations.GetEvent(createdEvent.Id);
			Assert.AreEqual(createdEvent, retrievedEvent);
		}

		[TestMethod]
		public void NotFound() {
			var notFoundEvent = _operations.GetEvent(Guid.NewGuid());
			Assert.IsNull(notFoundEvent);
		}

		[TestMethod]
		public void BasicUpdate() {
			var createdEvent = CreateEvent();
			var updatedEvent = new Event {
				Id = createdEvent.Id,
				Name = "Singalot Everett - Updated",
				VenueId = VenueId,
				EventStart = DateTime.UtcNow.AddDays(12),
				EventEnd = DateTime.UtcNow.AddDays(13),
				ForSaleStart = DateTime.UtcNow.AddDays(1),
				ForSaleEnd = DateTime.UtcNow.AddDays(12),
				Description = "A concert by Singalot outside Everett"
			};
			var updatedResult = _operations.UpdateEvent(updatedEvent);
			Assert.AreEqual(updatedEvent, updatedResult);
			var retrievedEvent = _operations.GetEvent(createdEvent.Id);
			Assert.AreEqual(updatedResult, retrievedEvent);
		}

		[TestMethod]
		public void TicketTypeCreate() {
			var createdEvent = CreateEvent();
			var ticketTypeA = new TicketType {
				Name = "VIP",
				Price = 100.00m,
				Seats = ["A1", "A2"]
			};

			var createdTicketType1 = _operations.CreateTicketType(createdEvent.Id, ticketTypeA);
			Assert.AreEqual(ticketTypeA.Name, createdTicketType1.Name);

			var ticketTypeB = new TicketType {
				Name = "General",
				Price = 50.00m,
				Seats = ["B1", "B2"]
			};

			var createdTicketType2 = _operations.CreateTicketType(createdEvent.Id, ticketTypeB);
			Assert.AreEqual(ticketTypeB.Name, createdTicketType2.Name);

			var retrievedEvent = _operations.GetEvent(createdEvent.Id);
			Assert.IsTrue(retrievedEvent.TicketTypeIds.Contains(createdTicketType1.Id));
			Assert.IsTrue(retrievedEvent.TicketTypeIds.Contains(createdTicketType2.Id));
		}


		private Event CreateEvent() {
			var newEvent = GenerateEvent();
			return _operations.CreateEvent(newEvent);
		}

		private Event GenerateEvent() {
			return new Event {
				Name = "Singalot Everett",
				VenueId = VenueId,
				EventStart = DateTime.UtcNow.AddDays(10),
				EventEnd = DateTime.UtcNow.AddDays(11),
				ForSaleStart = DateTime.UtcNow,
				ForSaleEnd = DateTime.UtcNow.AddDays(11),
				Description = "A concert by Singalot in Everett"
			};
		}
	}
}
