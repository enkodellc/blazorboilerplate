using Blazor.Fluxor;
using BlazorBoilerplate.Shared.FlightFinder;
using System.Collections.Generic;

namespace BlazorBoilerplate.CommonUI.FlightFinder.Store
{
	public class SearchCompleteReducer : Reducer<AppState, SearchCompleteAction>
	{
		public override AppState Reduce(AppState state, SearchCompleteAction action)
		{
			return new AppState(
				searchInProgress: false,
				searchResults: action.SearchResults,
				shortlist: state.Shortlist,
				airports: state.Airports);
		}
	}
}
