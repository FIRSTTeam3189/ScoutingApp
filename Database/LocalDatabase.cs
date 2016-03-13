using System;
using System.Collections.Generic;
using Scouty.Models.Local;

using SQLite.Net;
using SQLite.Net.Interop;
using SQLiteNetExtensions.Extensions;

namespace Scouty.Database
{
	public class LocalDatabase
	{
		public static LocalDatabase Database { get; private set; }

		public static void Initialize(string path, ISQLitePlatform platform){
			if (Database != null)
				throw new InvalidProgramException ("Database was already initialized");

			Database = new LocalDatabase (path, platform);
		}

		public SQLiteConnection Connection { get; private set; }

		private LocalDatabase (string dbPath, ISQLitePlatform platform)
		{
			Connection = new SQLiteConnection (platform, dbPath);

			// Create the tables
			Connection.CreateTable<Team>();
			Connection.CreateTable<RobotPerformance> ();
			Connection.CreateTable<RobotEvent> ();
			Connection.CreateTable<EventTeam> ();
			Connection.CreateTable<Event> ();
			Connection.CreateTable<Match> ();
		}

		/// <summary>
		/// Queries a team by their number
		/// </summary>
		/// <returns>The team, if its in the DB.</returns>
		/// <param name="teamNumber">Team number.</param>
		public Team QueryTeam(int teamNumber, bool getChildren = true){
			var team = (from t in Connection.Table<Team> ()
			            where t.TeamNumber == teamNumber
			            select t).FirstOrDefault ();
			if (team != null && getChildren)
				Connection.GetChildren<Team> (team);

			return team;
		}

		/// <summary>
		/// Gets all of the robot performances pertainning to a match
		/// </summary>
		/// <returns>The Performances</returns>
		/// <param name="eventCode">Event code for match.</param>
		/// <param name="matchNumber">Match number.</param>
		/// <param name="type">Type of match.</param>
		public IEnumerable<RobotPerformance> QueryMatch(string eventCode, int matchNumber, MatchType type){
			return (from t in Connection.Table<RobotPerformance> ()
			        where t.EventCode == eventCode && t.MatchNumber == matchNumber && t.MatchType == type
			        select t);
		}

		/// <summary>
		/// Gets all of the matches at an event.
		/// Will throw an exception if the event doesn't exist
		/// </summary>
		/// <returns>The matches.</returns>
		/// <param name="eventCode">Event code.</param>
		/// <param name = "year">Year of event</param>
		public IEnumerable<Match> QueryMatches(string eventCode, int year){
			var ev = Connection.Table<Event> ().Where(x => x.EventCode == eventCode && x.Year == year).FirstOrDefault();

			// Have to get matches from server
			if (ev == null)
				return null;

			Connection.GetChildren<Event> (ev);
			return ev.Matches;
		}

		public IEnumerable<RobotPerformance> GetPerformances(string eventCode){
			return Connection.GetAllWithChildren<RobotPerformance> (x => x.EventCode == eventCode, true);
		}

		/// <summary>
		/// Adds the team, if it doesnt already exist. Sets the Id if it already exists
		/// </summary>
		/// <param name="team">Team.</param>
		public void AddTeam(Team team){
			// Add the team if it doesn't exist
			Team existTeam = QueryTeam(team.TeamNumber);

			if (existTeam == null)
				Connection.Insert (team);
			else {
				team.Id = existTeam.Id;
			}
		}

		/// <summary>
		/// Gets the event if it exists
		/// </summary>
		/// <returns>The event.</returns>
		/// <param name="eventCode">Event code.</param>
		/// <param name="year">Year.</param>
		/// <param name = "readTeamsInfo">Reads all of the info stored about the team also</param>
		public Event QueryEvent(string eventCode, int year, bool readTeamsInfo = false){
			var ev = (from e in Connection.Table<Event> ()
			        where e.Year == year && e.EventCode == eventCode
			        select e).FirstOrDefault ();

			if (ev != null)
				Connection.GetChildren<Event> (ev, readTeamsInfo);

			return ev;
		}

		/// <summary>
		/// Queries the event teams.
		/// </summary>
		/// <returns>The event teams.</returns>
		/// <param name="eventCode">Event code.</param>
		/// <param name="year">Year.</param>
		/// <param name="readTeamsInfo">If set to <c>true</c> read teams info.</param>
		public IEnumerable<Team> QueryEventTeams(string eventCode, int year, bool readTeamsInfo = false){
			var ev = (from e in Connection.Table<Event> ()
			          where e.Year == year && e.EventCode == eventCode
			          select e).First ();

			Connection.GetChildren<Event> (ev, readTeamsInfo);

			return ev.Teams;
		}
	}
}

