namespace TicketManagementTests {
	public static class AssertExtensions {
		public static void TimesAreWithinASecond(this Assert assert, DateTime expected, DateTime actual) {
			var delta = Math.Abs(expected.Ticks - actual.Ticks);
			Assert.IsTrue(delta < TimeSpan.TicksPerSecond, $"The times were not within one second. The expected time was {expected:O}, but the actual time was {actual:O}.");
		}
	}
}
