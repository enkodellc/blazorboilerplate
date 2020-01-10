namespace BlazorBoilerplate.Shared.FlightFinder
{
	public class Itinerary
	{
		public FlightSegment Outbound { get; set; }
		public FlightSegment Return { get; set; }
		public decimal Price { get; set; }

		public double TotalDurationHours
			=> Outbound.DurationHours + Return.DurationHours;

		public string AirlineName =>
			(Outbound.Airline == Return.Airline) ? Outbound.Airline : "Multiple airlines";
	}
}
