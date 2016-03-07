using System;
using System.Collections.Generic;
using Scouty.Models.Local;

using SQLite.Net;
using SQLite.Net.Interop;

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
		}

		/// <summary>
		/// Queries a team by their number
		/// </summary>
		/// <returns>The team, if its in the DB.</returns>
		/// <param name="teamNumber">Team number.</param>
		public Team QueryTeam(int teamNumber){
			return (from t in Connection.Table<Team> ()
			        where t.TeamNumber == teamNumber
			        select t).FirstOrDefault ();
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
	}
}

