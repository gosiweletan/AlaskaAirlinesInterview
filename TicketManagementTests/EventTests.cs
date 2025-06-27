using TicketManagementSystem;
using TicketManagementSystem.Models;

namespace TicketManagementTests {
	[TestClass]
	public sealed class EventTests : TestBase {
		[TestInitialize]
		public void TestInitialize() {
			base.CreateOperationsAndVenue();
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
			var updatedResult = _operations.UpdateEvent(createdEvent.Id, updatedEvent);
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
			var allTickets = _operations.GetEventTickets(createdEvent.Id, default, default, default);
			Assert.AreEqual(3, allTickets.Count());
		}

		[TestMethod]
		public void BasicTicketGetAvailable() {
			var createdEvent = CreateEvent(addTicketType: true);
			var tickets = _operations.GetEventTickets(createdEvent.Id, default, default, default);

			Assert.IsNotNull(tickets);
			Assert.IsTrue(tickets.Count() == 2, "Expected tickets to be created for the event.");
			var ticketType = _operations.GetEventTicketTypes(createdEvent.Id).First();
			var availableTickets = tickets.Where(t => t.TicketTypeId == ticketType.Id && t.Status == TicketStatus.Available).ToList();
			Assert.IsTrue(availableTickets.Count == 2, "Expected two available tickets for the VIP ticket type.");
		}

		[TestMethod]
		public void HappyPathEndToEnd() {
			TicketTypeUpdate();
			var foundEvent = _operations.GetEvents(default, default).FirstOrDefault();
			var availableTickets = _operations.GetEventTickets(foundEvent.Id, TicketStatus.Available, default, default);
			var ticketsAvailable = availableTickets.Count();
			var availableTicket = availableTickets.FirstOrDefault();
			var user = Guid.NewGuid();
			var reservation = _operations.CreateTicketReservation(availableTicket.Id, new TicketReservation { UserId = user });
			var retrievedReservation = _operations.GetTicketReservation(availableTicket.Id, user);
			Assert.IsNotNull(retrievedReservation, "Reservation should be created successfully.");
			var purchase = _operations.CreateTicketPurchase(availableTicket.Id, user, Guid.NewGuid().ToString(), 120.00m);
			Assert.IsNotNull(purchase, "Purchase should be created successfully.");
			var retrievedPurchase = _operations.GetTicketPurchase(availableTicket.Id);
			Assert.IsNotNull(retrievedPurchase, "Purchase should be retrievable.");
			availableTickets = _operations.GetEventTickets(foundEvent.Id, TicketStatus.Available, default, default);
			Assert.AreEqual(ticketsAvailable - 1, availableTickets.Count(), "One ticket should be purchased, reducing the available count.");
		}
	}
}
