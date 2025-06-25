namespace TicketManagementSystem.Models {
	public class Event {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public string? Description { get; set; }
		public Guid VenueId { get; set; }
		public DateTime EventStart { get; set; }
		public DateTime EventEnd { get; set; }
		public DateTime ForSaleStart { get; set; }
		public DateTime ForSaleEnd { get; set; }

		public Guid[]? TicketTypeIds { get; set; }
	}
}
