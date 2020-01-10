using System;

namespace BlazorBoilerplate.Shared.FlightFinder
{
	public class SearchCriteria
	{
		public string FromAirport { get; set; }
		public string ToAirport { get; set; }
		public DateTime OutboundDate { get; set; }
		public DateTime ReturnDate { get; set; }
		public TicketClass TicketClass { get; set; }

		public SearchCriteria(string fromAirport, string toAirport)
		{
			FromAirport = fromAirport;
			ToAirport = toAirport;
			OutboundDate = DateTime.Now.Date;
			ReturnDate = OutboundDate.AddDays(7);
		}
	}
}
