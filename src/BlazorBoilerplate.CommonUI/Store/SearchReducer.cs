using Blazor.Fluxor;
using System.Collections.Generic;
using BlazorBoilerplate.Shared.FlightFinder;

namespace BlazorBoilerplate.CommonUI.FlightFinder.Store
{
	public class SearchReducer : Reducer<AppState, SearchAction>
	{
		public override AppState Reduce(AppState state, SearchAction action)
		{
			return new AppState(
				searchInProgress: true,
				searchResults: new List<Itinerary>(),
				shortlist: state.Shortlist,
				airports: state.Airports);
		}
	}
}
