using System;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLite.Net.Attributes;
using System.Collections.Generic;
using Scouty.Azure;
using System.Linq;

namespace Scouty.Models.Local
{
	[Table("Teams")]
	public class Team
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }

		[Column("team_number"), Unique]
		public int TeamNumber { get; set; }

		[MaxLength(100), Column("team_name")]
		public string TeamName { get; set; }

		[OneToMany(CascadeOperations = CascadeOperation.All), Column("robot_performances")]
		public List<RobotPerformance> Performances { get; set; }

		[ManyToMany(typeof(EventTeam), CascadeOperations = CascadeOperation.All), Column("events")]
		public List<Event> Events { get; set; }

	}

	public static class TeamExtensions {
		public static Team GetFromRemoteTeam(this ClientTeam team, IEnumerable<Event> events){
			return new Team {
				TeamName = team.NickName,
				TeamNumber = team.TeamNumber,
				Performances = team.TeamPerformance?.Select(x => x.GetFromRemote()).ToList() ?? new List<RobotPerformance>(),
				Events = new List<Event> (events)
			};
		}
	}
}

