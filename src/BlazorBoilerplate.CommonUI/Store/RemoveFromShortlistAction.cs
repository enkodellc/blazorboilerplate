using Blazor.Fluxor;
using BlazorBoilerplate.Shared.FlightFinder;

namespace BlazorBoilerplate.CommonUI.FlightFinder.Store
{
	public class RemoveFromShortlistAction
	{
		public readonly Itinerary Itinerary;

		public RemoveFromShortlistAction(Itinerary itinerary)
		{
			Itinerary = itinerary;
		}
	}
}
