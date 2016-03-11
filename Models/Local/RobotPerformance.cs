using System;

using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using Scouty.Azure;

namespace Scouty.Models.Local
{
	[Table("RobotPerformances")]
	public class RobotPerformance
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }

		[ForeignKey(typeof(Team)), Column("team_id")]
		public int TeamId { get; set; }

		[Column("match_number")]
		public int MatchNumber { get; set; }

		[Column("match_type")]
		public MatchType MatchType { get; set; }

		[MaxLength(80), Column("event_code")]
		public string EventCode { get; set; }

		[ManyToOne]
		public Team Team { get; set; }

		[OneToMany(CascadeOperations = CascadeOperation.All), Column("events")]
		public List<RobotEvent> Events { get; set; }
	}

	public static class RobotPerformanceExtensions {
		public static RobotPerformance GetFromRemote(this ClientPerformance performance){
			return new RobotPerformance{
				
			};
		}
	}
}

