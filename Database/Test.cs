using System;
using SQLite.Net;
using SQLiteNetExtensions.Extensions;
using Scouty.Utility;
using Scouty.Models.Local;
using System.Collections.Generic;


namespace Scouty.Database
{
	public static class Test
	{
		static readonly Logger logger = new Logger (typeof(Test));
		public static void TestCreate(){
			var connection = LocalDatabase.Database.Connection;

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

		public static void TestCreateEvent(){
			// Create the event, if needed
			var db = LocalDatabase.Database;

			var ev = db.QueryEvent ("cada", 2016, true);
			if (ev != null) {
				logger.Info ($"Found Event! {ev.EventCode} {ev.Year} Participating Teams...");
				foreach (var team in ev.Teams)
					logger.Info ($"{team.TeamNumber} : {team.TeamName}, Attending {team.Events.Count} Events");

				return;
			}

			// Lets create the test event
			var teams = new List<Team> () {
				new Team () { TeamName = "Citris Circuits", TeamNumber = 1678 },
				new Team () { TeamName = "Cheesy poofs", TeamNumber = 254 },
				new Team () { TeamName = "Raptor Force", TeamNumber = 1662 },
				new Team () { TeamName = "Robo Vikes", TeamNumber = 701 },
				new Team () { TeamName = "Team Eight", TeamNumber = 8 },
				new Team () { TeamName = "Juggernaughts", TeamNumber = 1 },
				new Team () { TeamName = "Beachbots", TeamNumber = 301 },
				new Team () { TeamName = "Fembots", TeamNumber = 264 },
				new Team () { TeamName = "GRT", TeamNumber = 114 },
				new Team () { TeamName = "Bird Brains", TeamNumber = 1701 },
			};

			// Insert all of the teams
			db.Connection.InsertAll(teams);

			foreach (var team in teams)
				logger.Debug ("Id: " + team.Id);

			// Select CB if they exist
			var cb = db.QueryTeam(3189);
			if (cb != null)
				teams.Add (cb);

			// Now insert all of the teams into a new event
			ev = new Event() {
				EventCode = "cada",
				Year = 2016
			};

			db.Connection.Insert (ev);

			// Now update with teams
			ev.Teams = teams;

			db.Connection.UpdateWithChildren (ev);
		}

		public static void DeleteAllData(){
			var db = LocalDatabase.Database;

			db.Connection.DeleteAll<Event> ();
			db.Connection.DeleteAll<EventTeam> ();
			db.Connection.DeleteAll<Match> ();
			db.Connection.DeleteAll<RobotEvent> ();
			db.Connection.DeleteAll<RobotPerformance> ();
			db.Connection.DeleteAll<Team> ();
		}

		public static void DropAllTables(){
			var db = LocalDatabase.Database;

			db.Connection.DropTable<EventTeam> ();
			db.Connection.DropTable<RobotEvent> ();
			db.Connection.DropTable<RobotPerformance> ();
			db.Connection.DropTable<Match> ();
			db.Connection.DropTable<Event> ();
			db.Connection.DropTable<Team> ();
		}

	}
}

