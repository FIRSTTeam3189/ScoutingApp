using System;
using System.Linq;
using Scouty.Models.Local;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Scouty.Utility;

namespace Scouty.Azure
{
	public static class PerformanceManager
	{
		static readonly Logger logger = new Logger (typeof(PerformanceManager));
		// POST REQUEST
		const string POST_PERFORMANCE = "performances/PostPerformances";
		// GET REQUEST, EventCode
		const string GET_PERFORMANCES = "performances/GetPerformances";

		public static async Task<bool> PostPerformances(List<RobotPerformance> performances){
			try {
				await AzureManager.InvokeApiAsync<List<ClientPerformance>, string>(POST_PERFORMANCE, performances.Select(x => x.ToRemote()).ToList());
				return true;
			} catch (MobileServiceInvalidOperationException e){
				if (e.Response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
					logger.Error ("Server Failure");
				else
					logger.Error ("Something Happened: " + e.Response.StatusCode + await e.Response.Content.ReadAsStringAsync ());
			}

			return false;
		}

		public static async Task<List<ClientPerformance>> GetPerformances(string eventCode){
			try {
				return await AzureManager.InvokeGetApiAsync<List<ClientPerformance>>(GET_PERFORMANCES, new Dictionary<string, string>(){{"EventCode", eventCode}});
			} catch (MobileServiceInvalidOperationException e){
				if (e.Response.StatusCode == System.Net.HttpStatusCode.InternalServerError) {
					logger.Error ("Server Failure: " + await e.Response.Content.ReadAsStringAsync ());
				} else {
					logger.Error ("Something else happened: " + e.Response.StatusCode + await e.Response.Content.ReadAsStringAsync ());
				}
			}

			return null;
		}
	}

	public class ClientRobotEvent {
		public EventType EventType { get; set; }
		public EventTime EventTime { get; set; }
	}

	public class ClientPerformance {
		public string Id { get; set; }
		public string EventCode { get; set; }
		public int TeamId { get; set; }
		public int MatchNumber { get; set; }
		public MatchType MatchType { get; set; }
		public List<ClientRobotEvent> Events { get; set; }
		public DateTimeOffset? LastUpdated { get; set; }
	}
}

