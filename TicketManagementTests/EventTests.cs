using TicketManagementSystem;
using TicketManagementSystem.Models;

namespace TicketManagementTests {
	[TestClass]
	public sealed class EventTests : TestBase {
		[TestInitialize]
		public void TestInitialize() {
			base.CreateVenue();
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
				VenueId = _venueId,
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
			var ticketTypeA = GenerateVipTicketType();
			var createdTicketType1 =  _operations.CreateTicketType(createdEvent.Id, ticketTypeA);
			Assert.AreEqual(ticketTypeA.Name, createdTicketType1.Name);

			var ticketTypeB = new TicketType {
				Name = "General",
				Price = 50.00m,
				Seats = ["B1", "B2"]
			};
			var createdTicketType2 = _operations.CreateTicketType(createdEvent.Id, ticketTypeB);
			Assert.AreEqual(ticketTypeB.Name, createdTicketType2.Name);

			var allTicketTypes = _operations.GetEventTicketTypes(createdEvent.Id);
			Assert.AreEqual(2, allTicketTypes.Count());

			var retrievedTicketType = _operations.GetEventTicketType(createdEvent.Id, createdTicketType1.Id);
			Assert.AreEqual(createdTicketType1, retrievedTicketType);
		}

		[TestMethod]
		public void TicketTypeUpdate() {
			var createdEvent = CreateEvent(addTicketType: true);
			var createdTicketType = _operations.GetEventTicketTypes(createdEvent.Id).First();
			var updatedTicketType = new TicketType {
				Id = createdTicketType.Id,
				Name = "VIP - Updated",
				Price = 120.00m,
				Seats = ["A1", "A2", "B1"]
			};
			var updatedResult = _operations.UpdateTicketType(createdEvent.Id, createdTicketType.Id, updatedTicketType);
			Assert.AreEqual(updatedTicketType, updatedResult);
		}
	}
}
