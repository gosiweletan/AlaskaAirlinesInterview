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
		public List<Guid>? TicketTypeIds { get; set; }
		public List<Guid>? TicketIds { get; set; }

		public override bool Equals(object? obj) {
			if (obj is Event otherEvent) {
				return Id == otherEvent.Id &&
					Name == otherEvent.Name &&
					Description == otherEvent.Description &&
					VenueId == otherEvent.VenueId &&
					EventStart == otherEvent.EventStart &&
					EventEnd == otherEvent.EventEnd &&
					ForSaleStart == otherEvent.ForSaleStart &&
					ForSaleEnd == otherEvent.ForSaleEnd &&
					((TicketTypeIds == null && otherEvent.TicketTypeIds == null) || (TicketTypeIds?.SequenceEqual(otherEvent.TicketTypeIds ?? []) ?? false)) &&
					((TicketIds == null && otherEvent.TicketIds == null) || (TicketIds?.SequenceEqual(otherEvent.TicketIds ?? []) ?? false));
			}

			return false;
		}
	}
}
