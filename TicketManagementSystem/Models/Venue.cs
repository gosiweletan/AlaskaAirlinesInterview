namespace TicketManagementSystem.Models {
	public class Venue {
		public Guid Id { get; set; }

		public required string Name { get; set; }

		public string[] Seats { get; set; }

		public override bool Equals(object? obj) {
			if (obj is Venue otherVenue) {
				return Id == otherVenue.Id && Name == otherVenue.Name && Seats.SequenceEqual(otherVenue.Seats);
			}
			return false;
		}

		public override int GetHashCode() {
			return HashCode.Combine(Id, Name, Seats);
		}
	}
}
