using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.MobileServices;
using Scouty.Utility;

namespace Scouty.Azure
{
	public static class TeamManager
	{
		static readonly Logger logger = new Logger (typeof(TeamManager));
		const string GET_TEAM = "teams/GetTeam";

		public static async Task<ClientTeam> GetTeam(int teamNumber){
			try {
				return await AzureManager.InvokeApiAsync<TeamInfoRequest, ClientTeam>(GET_TEAM, 
					new TeamInfoRequest { TeamNumber = teamNumber });
			} catch (MobileServiceInvalidOperationException e){
				if (e.Response.StatusCode == System.Net.HttpStatusCode.InternalServerError) {
					logger.Error ("Server Failure: " + e.Message);
				} else {
					logger.Error ("Something Happened: " + e.Response.StatusCode);
				}
			}

			return null;
		}
	}

	public class TeamInfoRequest {
		public int TeamNumber { get; set; }
	}

	public class ClientTeam {
		public int TeamNumber { get; set; }
		public int RookieYear { get; set; }
		public string NickName { get; set; }
		public string TeamLocation { get; set; }
		public ICollection<ClientPerformance> TeamPerformance { get; set; }
	}

	public class ClientPerformance {
		
	}
}

