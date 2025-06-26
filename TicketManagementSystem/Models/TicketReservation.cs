namespace TicketManagementSystem.Models {
	public class TicketReservation {
		public Guid? Id { get; set; }
		public required Guid UserId { get; set; }
		public DateTime ReservedUntil { get; set; }
	}
}
