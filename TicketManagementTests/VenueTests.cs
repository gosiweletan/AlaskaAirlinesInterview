using TicketManagementSystem;
using TicketManagementSystem.Models;

namespace TicketManagementTests {
	[TestClass]
	public sealed class VenueTests {
		private readonly Operations _operations = new();

		[TestMethod]
		public void BasicCreate() {
			var newVenue = new Venue { Name = "MiniStadium", Seats = ["A1", "A2", "B1", "B2"] };
			var createdVenue = _operations.CreateVenue(newVenue);
			var defaultGuid = new Guid();
			Assert.IsTrue(createdVenue.Id != defaultGuid);
			var retrievedVenue = _operations.GetVenue(createdVenue.Id);
			Assert.AreEqual(createdVenue, retrievedVenue);
		}
	}
}
