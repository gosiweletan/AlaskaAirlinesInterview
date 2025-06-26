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
		public void BasicGetEventTickets() {
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
	}
}
