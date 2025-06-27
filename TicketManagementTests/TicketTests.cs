using TicketManagementSystem;
using TicketManagementSystem.Models;

namespace TicketManagementTests {

	[TestClass]
	public sealed class TicketTests : TestBase {

		private Event _testEvent;

		[TestInitialize]
		public void TestInitialize() {
			base.CreateVenue();
			_testEvent = base.CreateEvent(true);
		}

		[TestMethod]
		public void BasicTicketTypeUpdateChangesTickets() {
			var tickets = _operations.GetEventTickets(_testEvent.Id);
			Assert.IsNotNull(tickets);
			Assert.IsTrue(tickets.Count() == 2, "Expected tickets to be created for the event.");

			var ticketType = _operations.GetEventTicketTypes(_testEvent.Id).First();
			ticketType.Seats = ["A1"];
			var updatedTicketType = _operations.UpdateTicketType(_testEvent.Id, ticketType.Id, ticketType);
			var updatedTickets = _operations.GetEventTickets(_testEvent.Id);
			Assert.IsNotNull(updatedTickets);
			Assert.IsTrue(updatedTickets.Count() == 1, "Expected tickets to be updated based on the ticket type changes.");
		}

		[TestMethod]
		public void BasicTicketReservation() {
			var ticket = _operations.GetEventTickets(_testEvent.Id).First();
			var userId = Guid.NewGuid();
			Assert.IsNull(_operations.GetTicketReservation(ticket.Id, userId), "There should be no reservation for the ticket initially.");
			var ticketReservation = new TicketReservation { UserId = userId };
			var createdReservation = _operations.CreateTicketReservation(ticket.Id, ticketReservation);
			Assert.That.TimesAreWithinASecond(createdReservation.ReservedUntil, DateTime.UtcNow.AddMinutes(10));
			Assert.ThrowsException<InvalidOperationException>(() => _operations.CreateTicketReservation(ticket.Id, ticketReservation), "Cannot reserve the same ticket again without releasing it first.");
		}

		[TestMethod]
		public void TicketReservationOperations() {
			var ticket = _operations.GetEventTickets(_testEvent.Id).First();
			var ticketReservation = new TicketReservation { UserId = Guid.NewGuid() };
			var createdReservation = _operations.CreateTicketReservation(ticket.Id, ticketReservation);
			Assert.That.TimesAreWithinASecond(createdReservation.ReservedUntil, DateTime.UtcNow.AddMinutes(10));
			var newReservation = new TicketReservation { UserId = Guid.NewGuid() };
			Assert.ThrowsException<InvalidOperationException>(() => _operations.CreateTicketReservation(ticket.Id, newReservation), "Cannot reserve the same ticket for a different user without releasing it first.");
			_operations.DeleteTicketReservation(ticket.Id, createdReservation.UserId);

			// Now succeeds. No exception thrown.
			_operations.CreateTicketReservation(ticket.Id, newReservation);
		}

		[TestMethod]
		public void BasicTicketPurchase() {
			var ticket = _operations.GetEventTickets(_testEvent.Id).First();
			var ticketPurchase = _operations.CreateTicketPurchase(ticket.Id, Guid.NewGuid(), Guid.NewGuid().ToString(), 100.00m);
			var purchasedTicket = _operations.GetEventTicket(_testEvent.Id, ticket.Id);
			Assert.AreEqual(TicketStatus.Purchased, purchasedTicket.Status, "The ticket status should be 'Purchased' after a successful purchase.");
			var retrievedPurchase = _operations.GetTicket(purchasedTicket.Id);
			Assert.AreEqual(ticketPurchase.PurchaseToken, retrievedPurchase.PurchaseToken, "The purchase token should match the one used during the purchase.");
		}
	}
}
