using BlazorBoilerplate.Shared.FlightFinder;

namespace BlazorBoilerplate.Server.FlightFinder
{
	public class SampleData
	{
		public readonly static Airport[] Airports = new[]
		{
			new Airport { Code = "ATL", DisplayName = "Hartsfield–Jackson Atlanta International" },
			new Airport { Code = "PEK", DisplayName = "Beijing Capital International" },
			new Airport { Code = "DXB", DisplayName = "Dubai International" },
			new Airport { Code = "LAX", DisplayName = "Los Angeles International" },
			new Airport { Code = "HND", DisplayName = "Tokyo Haneda International" },
			new Airport { Code = "ORD", DisplayName = "O'Hare International" },
			new Airport { Code = "LHR", DisplayName = "London Heathrow" },
			new Airport { Code = "HKG", DisplayName = "Hong Kong International" },
			new Airport { Code = "PVG", DisplayName = "Shanghai Pudong International" },
			new Airport { Code = "CDG", DisplayName = "Charles de Gaulle" },
			new Airport { Code = "DFW", DisplayName = "Dallas/Fort Worth International" },
			new Airport { Code = "AMS", DisplayName = "Amsterdam Schiphol" },
			new Airport { Code = "FRA", DisplayName = "Frankfurt" },
			new Airport { Code = "IST", DisplayName = "Istanbul Atatürk" },
			new Airport { Code = "CAN", DisplayName = "Guangzhou Baiyun International" },
			new Airport { Code = "JFK", DisplayName = "John F. Kennedy International" },
			new Airport { Code = "SIN", DisplayName = "Singapore Changi" },
			new Airport { Code = "DEN", DisplayName = "Denver International" },
			new Airport { Code = "ICN", DisplayName = "Seoul Incheon International" },
			new Airport { Code = "BKK", DisplayName = "Suvarnabhumi" },
			new Airport { Code = "DEL", DisplayName = "Indira Gandhi International" },
			new Airport { Code = "CGK", DisplayName = "Soekarno–Hatta International" },
			new Airport { Code = "SFO", DisplayName = "San Francisco International" },
			new Airport { Code = "KUL", DisplayName = "Kuala Lumpur International" },
			new Airport { Code = "MAD", DisplayName = "Madrid Barajas" },
			new Airport { Code = "LAS", DisplayName = "McCarran International" },
			new Airport { Code = "CTU", DisplayName = "Chengdu Shuangliu International" },
			new Airport { Code = "SEA", DisplayName = "Seattle-Tacoma International" },
			new Airport { Code = "BOM", DisplayName = "Chhatrapati Shivaji International" },
			new Airport { Code = "MIA", DisplayName = "Miami International" },
			new Airport { Code = "CLT", DisplayName = "Charlotte Douglas International" },
			new Airport { Code = "YYZ", DisplayName = "Toronto Pearson International" },
			new Airport { Code = "BCN", DisplayName = "Barcelona–El Prat" },
			new Airport { Code = "PHX", DisplayName = "Phoenix Sky Harbor International" },
			new Airport { Code = "LGW", DisplayName = "London Gatwick" },
			new Airport { Code = "TPE", DisplayName = "Taiwan Taoyuan International" },
			new Airport { Code = "MUC", DisplayName = "Munich" },
			new Airport { Code = "SYD", DisplayName = "Sydney Kingsford-Smith" },
			new Airport { Code = "KMG", DisplayName = "Kunming Changshui International" },
			new Airport { Code = "SZX", DisplayName = "Shenzhen Bao'an International" },
			new Airport { Code = "MCO", DisplayName = "Orlando International" },
			new Airport { Code = "FCO", DisplayName = "Leonardo da Vinci–Fiumicino" },
			new Airport { Code = "IAH", DisplayName = "George Bush Intercontinental" },
			new Airport { Code = "MEX", DisplayName = "Benito Juárez International" },
			new Airport { Code = "SHA", DisplayName = "Shanghai Hongqiao International" },
			new Airport { Code = "EWR", DisplayName = "Newark Liberty International" },
			new Airport { Code = "MNL", DisplayName = "Ninoy Aquino International" },
			new Airport { Code = "NRT", DisplayName = "Narita International" },
			new Airport { Code = "MSP", DisplayName = "Minneapolis/St Paul International" },
			new Airport { Code = "DOH", DisplayName = "Hamad International" },
		};

		public readonly static string[] Airlines = new[]
		{
			"American Airlines",
			"British Airways",
			"Delta",
			"Emirates",
			"Etihad",
			"JetBlue",
			"KLM",
			"Singapore Airways",
			"Qantas",
			"Virgin Atlantic",
		};
	}
}
