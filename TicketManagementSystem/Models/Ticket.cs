using System.ComponentModel.DataAnnotations;

namespace TicketManagementSystem.Models {
	public class Ticket {
		[Key]
		public Guid Id { get; set; }
		public Guid EventId { get; set; }
		public Guid TicketTypeId { get; set; }
		public required string Seat { get; set; }
		public TicketStatus Status {
			get {
				if (PurchaseToken != null) {
					return TicketStatus.Sold;
				}
				else if (ReservedUntil.HasValue && ReservedUntil > DateTime.UtcNow) {
					return TicketStatus.Reserved;
				}
				else {
					return TicketStatus.Available;
				}
			}
		}
		public Guid? Owner { get; set; }
		public DateTime? ReservedUntil { get; set; }
		public string? PurchaseToken { get; set; }
		public decimal? PurchasePrice { get; set; }

		public enum TicketStatus {
			Available,
			Reserved,
			Sold
		}

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
