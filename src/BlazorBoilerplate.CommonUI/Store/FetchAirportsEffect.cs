using Blazor.Fluxor;
using BlazorBoilerplate.Shared.FlightFinder;
using Microsoft.AspNetCore.Components;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorBoilerplate.CommonUI.FlightFinder.Store
{
	public class FetchAirportsEffect : Effect<FetchAirportsAction>
	{
		private readonly HttpClient HttpClient;

		public FetchAirportsEffect(HttpClient httpClient)
		{
			HttpClient = httpClient;
		}

		protected async override Task HandleAsync(FetchAirportsAction action, IDispatcher dispatcher)
		{
			Airport[] airports = Array.Empty<Airport>();
			try
			{
				airports = await HttpClient.GetJsonAsync<Airport[]>("api/airports");
			}
			catch
			{
				// Should really dispatch an error action
			}
			var completeAction = new FetchAirportsCompleteAction(airports);
			dispatcher.Dispatch(completeAction);
		}
	}
}
