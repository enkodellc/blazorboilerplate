using BlazorBoilerplate.Server.FlightFinder;
using BlazorBoilerplate.Shared.FlightFinder;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorBoilerplate.FlightFinder.Server.Controllers
{
	[Route("api/[controller]")]
	public class FlightSearchController
	{
		public async Task<IEnumerable<Itinerary>> Search([FromBody] SearchCriteria criteria)
		{
			await Task.Delay(500); // Gotta look busy...

			var rng = new Random();
			return Enumerable.Range(0, rng.Next(1, 5)).Select(_ => new Itinerary
			{
				Price = rng.Next(100, 2000),
				Outbound = new FlightSegment
				{
					Airline = RandomAirline(),
					FromAirportCode = criteria.FromAirport,
					ToAirportCode = criteria.ToAirport,
					DepartureTime = criteria.OutboundDate.AddHours(rng.Next(24)).AddMinutes(5 * rng.Next(12)),
					ArrivalTime = criteria.OutboundDate.AddHours(rng.Next(24)).AddMinutes(5 * rng.Next(12)),
					DurationHours = 2 + rng.Next(10),
					TicketClass = criteria.TicketClass
				},
				Return = new FlightSegment
				{
					Airline = RandomAirline(),
					FromAirportCode = criteria.ToAirport,
					ToAirportCode = criteria.FromAirport,
					DepartureTime = criteria.ReturnDate.AddHours(rng.Next(24)).AddMinutes(5 * rng.Next(12)),
					ArrivalTime = criteria.ReturnDate.AddHours(rng.Next(24)).AddMinutes(5 * rng.Next(12)),
					DurationHours = 2 + rng.Next(10),
					TicketClass = criteria.TicketClass
				},
			});
		}

		private string RandomAirline()
			=> SampleData.Airlines[new Random().Next(SampleData.Airlines.Length)];
	}
}
