using Blazor.Fluxor;
using BlazorBoilerplate.Shared.FlightFinder;

namespace BlazorBoilerplate.CommonUI.FlightFinder.Store
{
	public class FetchAirportsCompleteAction
	{
		public readonly Airport[] Airports;

		public FetchAirportsCompleteAction(Airport[] airports)
		{
			Airports = airports;
		}
	}
}
