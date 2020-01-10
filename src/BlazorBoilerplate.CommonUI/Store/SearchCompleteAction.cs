using System;
using Blazor.Fluxor;
using BlazorBoilerplate.Shared.FlightFinder;

namespace BlazorBoilerplate.CommonUI.FlightFinder.Store
{
	public class SearchCompleteAction
	{
		public readonly Itinerary[] SearchResults;

		public SearchCompleteAction(Itinerary[] searchResults)
		{
			SearchResults = searchResults ?? Array.Empty<Itinerary>();
		}
	}
}
