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
		const string GET_EVENT_MATCHES = "events/GetMatches";
		const string GET_EVENT_TEAMS = "events/GetTeams";

		/// <summary>
		/// Refreshs the events for a given year
		/// </summary>
		/// <returns>The events.</returns>
		/// <param name="year">Year.</param>
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

		/// <summary>
		/// Teamses for event. Auto generated comment.
		/// </summary>
		/// <returns>List of teams.</returns>
		/// <param name="eventCode">Event code.</param>
		/// <param name="year">Year.</param>
		public static async Task<List<ClientTeam>> TeamsForEvent(string eventCode, int year){
			try {
				return await AzureManager.InvokeApiAsync<EventTeamsRequest, List<ClientTeam>>(GET_EVENT_TEAMS, new EventTeamsRequest(){
					EventCode = eventCode,
					Year = year
				});
			} catch (MobileServiceInvalidOperationException e){
				if (e.Response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
					logger.Error ("Something happened on the server: " + e.Message);
				else
					logger.Error ("Error: " + e.Response.StatusCode + " " + await e.Response.Content.ReadAsStringAsync ());
			}

			return null;
		}

		public static async Task<List<ClientMatch>> MatchesForEvent(string eventCode, int year){
			try {
				return await AzureManager.InvokeApiAsync<EventMatchesRequest, List<ClientMatch>>(GET_EVENT_MATCHES, new EventMatchesRequest(){
					Year = year,
					EventCode = eventCode
				});
			} catch (MobileServiceInvalidOperationException e){
				if (e.Response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
					logger.Error ("Something happened on the server: " + e.Response.StatusCode);
				else
					logger.Error ("Error: " + e.Response.StatusCode + " " + await e.Response.Content.ReadAsStringAsync ());
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

	public class EventMatchesRequest {
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

