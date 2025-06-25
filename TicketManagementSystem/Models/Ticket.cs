using System.ComponentModel.DataAnnotations;

namespace TicketManagementSystem.Models {
	public class Ticket {
		[Key]
		public Guid Id { get; set; }
		public Guid EventId { get; set; }
		public Guid TicketTypeId { get; set; }
		public required string Seat { get; set; }
		public string? PurchaseToken { get; set; }
		public string Status { get; set; } = "Available";
		public override bool Equals(object? obj) {
			if (obj is Ticket otherTicket) {
				return EventId == otherTicket.EventId && Seat == otherTicket.Seat;
			}

			return false;
		}
		public override int GetHashCode() {
			return Id.GetHashCode();
		}
	}
}
