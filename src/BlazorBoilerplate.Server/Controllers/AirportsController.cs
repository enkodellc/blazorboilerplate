using BlazorBoilerplate.Server.FlightFinder;
using BlazorBoilerplate.Shared.FlightFinder;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace BlazorBoilerplate.FlightFinder.Server.Controllers
{
	[Route("api/[controller]")]
	public class AirportsController : Controller
	{
		public IEnumerable<Airport> Airports()
		{
			return SampleData.Airports;
		}
	}
}
