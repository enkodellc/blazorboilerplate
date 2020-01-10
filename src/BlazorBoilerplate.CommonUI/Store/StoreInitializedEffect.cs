using Blazor.Fluxor;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBoilerplate.CommonUI.FlightFinder.Store
{
	public class StoreInitializedEffect : Effect<StoreInitializedAction>
	{
		private readonly HttpClient HttpClient;

		public StoreInitializedEffect(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}

		protected override Task HandleAsync(StoreInitializedAction action, IDispatcher dispatcher)
		{
			dispatcher.Dispatch(new FetchAirportsAction());
			return Task.CompletedTask;
		}
	}
}
