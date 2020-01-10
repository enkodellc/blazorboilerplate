using Blazor.Fluxor;
using BlazorBoilerplate.Shared.FlightFinder;

namespace BlazorBoilerplate.CommonUI.FlightFinder.Store
{
	public class AddToShortlistAction
	{
		public readonly Itinerary Itinerary;

		public AddToShortlistAction(Itinerary itinerary)
		{
			Itinerary = itinerary;
		}
	}
}
