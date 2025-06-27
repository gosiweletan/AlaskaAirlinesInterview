namespace TicketManagementSystem.Models {
	public class TicketPurchase {
		public required Guid PurchaserId { get; set; }
		public required string PurchaseToken { get; set; }
		public required decimal PurchasePrice { get; set; }
	}
}