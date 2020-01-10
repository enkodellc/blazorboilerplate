using Blazor.Fluxor;

namespace BlazorBoilerplate.CommonUI.FlightFinder.Store
{
	public class FetchAirportsCompleteReducer : Reducer<AppState, FetchAirportsCompleteAction>
	{
		public override AppState Reduce(AppState state, FetchAirportsCompleteAction action)
		{
			return new AppState(
				searchInProgress: state.SearchInProgress,
				searchResults: state.SearchResults,
				shortlist: state.Shortlist,
				airports: action.Airports);
		}
	}
}
