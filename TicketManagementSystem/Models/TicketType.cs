namespace TicketManagementSystem.Models {
	public class TicketType {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public decimal Price { get; set; }
		public string[]? Seats { get; set; }
	}
}
