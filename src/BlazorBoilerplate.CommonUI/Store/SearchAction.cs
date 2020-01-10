using Blazor.Fluxor;
using BlazorBoilerplate.Shared.FlightFinder;

namespace BlazorBoilerplate.CommonUI.FlightFinder.Store
{
	public class SearchAction
	{
		public readonly SearchCriteria SearchCriteria;

		public SearchAction(SearchCriteria searchCriteria)
		{
			SearchCriteria = searchCriteria;
		}
	}
}
