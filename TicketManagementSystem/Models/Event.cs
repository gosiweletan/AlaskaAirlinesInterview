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

		public override bool Equals(object? obj) {
			if (obj is Event otherEvent) {
				return Id == otherEvent.Id &&
					Name == otherEvent.Name &&
					Description == otherEvent.Description &&
					VenueId == otherEvent.VenueId &&
					EventStart == otherEvent.EventStart &&
					EventEnd == otherEvent.EventEnd &&
					ForSaleStart == otherEvent.ForSaleStart &&
					ForSaleEnd == otherEvent.ForSaleEnd;
			}

			return false;
		}
	}
}
