using TicketManagementSystem;
using TicketManagementSystem.Models;

namespace TicketManagementTests {
	[TestClass]
	public sealed class VenueTests {
		private readonly Operations _operations = new();

		[TestMethod]
		public void BasicCreateAndGet() {
			var newVenue = new Venue { Name = "MiniStadium", Seats = ["A1", "A2", "B1", "B2"] };
			var createdVenue = _operations.CreateVenue(newVenue);
			var defaultGuid = new Guid();
			Assert.IsTrue(createdVenue.Id != defaultGuid);
			var retrievedVenue = _operations.GetVenue(createdVenue.Id);
			Assert.AreEqual(createdVenue, retrievedVenue);
			var allVenues = _operations.GetVenues(1, 5);
			Assert.AreEqual(1, allVenues.Count(), "There should be one venue created.");
		}

		[TestMethod]
		public void BasicUpdateAndGet() {
			var newVenue = new Venue { Name = "MiniStadium", Seats = ["A1", "A2", "B1", "B2"] };
			var createdVenue = _operations.CreateVenue(newVenue);
			var retrievedVenue = _operations.GetVenue(createdVenue.Id);
			retrievedVenue.Name = "MiniStadium - Updated";
			retrievedVenue.Seats = ["A1", "A2", "B1", "B2", "C1"];
			var updatedVenue = _operations.UpdateVenue(createdVenue.Id, retrievedVenue);
			retrievedVenue = _operations.GetVenue(createdVenue.Id);
			Assert.AreEqual("MiniStadium - Updated", retrievedVenue.Name, "The venue name should be updated.");
			Assert.IsTrue(retrievedVenue.Seats.Contains("C1"), "The new seat C1 should be added to the venue.");
		}
	}
}
