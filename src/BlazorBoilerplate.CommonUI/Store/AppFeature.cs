using Blazor.Fluxor;

namespace BlazorBoilerplate.CommonUI.FlightFinder.Store
{
	public class AppFeature : Feature<AppState>
	{
		public override string GetName() => "App";
		protected override AppState GetInitialState() => new AppState();
	}
}
