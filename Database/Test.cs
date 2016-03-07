using System;
using SQLite.Net;
using Scouty.Utility;
using Scouty.Models.Local;


namespace Scouty.Database
{
	public static class Test
	{
		static readonly Logger logger = new Logger (typeof(Test));
		public static void TestCreate(){
			var connection = LocalDatabase.Database.Connection;
			connection.CreateTable<Team> ();

			var team = LocalDatabase.Database.QueryTeam (3189);
			if (team != null) {
				logger.Info ($"Found Team! {team.Id}, Name: {team.TeamName}, Number: {team.TeamNumber}");
				return;
			}
			
			var testTeam = new Team (){ 
				TeamNumber = 3189,
				TeamName = "Circuit Breakers"
			};

			logger.Info ("Creating Team...");

			connection.Insert (testTeam);
		}
	}
}

