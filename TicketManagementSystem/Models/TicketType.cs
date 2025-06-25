namespace TicketManagementSystem.Models {
	public class TicketType {
		public Guid Id { get; set; }
		public required string Name { get; set; }
		public decimal Price { get; set; }
		public string[]? Seats { get; set; }

		public override bool Equals(object? obj) {
			if (obj is TicketType otherTicketType) {
				return Id == otherTicketType.Id &&
					Name == otherTicketType.Name &&
					Price == otherTicketType.Price &&
					((Seats == null && otherTicketType.Seats == null) || (Seats?.SequenceEqual(otherTicketType.Seats ?? []) ?? false));
			}

			return false;
		}
	}
}
