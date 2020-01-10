using System;

namespace BlazorBoilerplate.Shared.FlightFinder
{
	public class FlightSegment
	{
		public string Airline { get; set; }
		public string FromAirportCode { get; set; }
		public string ToAirportCode { get; set; }
		public DateTime DepartureTime { get; set; }
		public DateTime ArrivalTime { get; set; }
		public double DurationHours { get; set; }
		public TicketClass TicketClass { get; set; }
	}
}
