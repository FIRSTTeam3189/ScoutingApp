using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Scouty.Utility;

namespace Scouty.Azure
{
	public static class EventManager
	{
		static readonly Logger logger = new Logger (typeof(EventManager));
		const string REFRESH_EVENTS = "events/Refresh";
		const string GET_EVENT_MATCHES = "events/GetEventMatches";
		const string GET_EVENT_TEAMS = "events/GetEventTeams";

		public static async Task<List<ClientEvent>> RefreshEvents(int year){
			try {
				return await AzureManager.InvokeApiAsync<RefreshEventRequest, List<ClientEvent>>(REFRESH_EVENTS, 
					new RefreshEventRequest { Year = year });
			} catch (MobileServiceInvalidOperationException e){
				if (e.Response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
					logger.Error ("Something happened on the server: " + e.Message);
				else {
					logger.Error ("Error: " + e.Response.StatusCode);
				}
			}

			return null;
		}
	}

	public class RefreshEventRequest {
		public int Year { get; set; }
	}

	public class EventTeamsRequest {
		public string EventCode { get; set; }
		public int Year { get; set; }
	}

	public class ClientEvent{
		public string Location { get; set; }
		public int Year { get; set; }
		public string EventCode { get; set; }
		public string Website { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public bool Official { get; set; }
	}
}

