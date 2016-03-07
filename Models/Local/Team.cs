using System;
using SQLite;
using SQLiteNetExtensions.Attributes;
using SQLite.Net.Attributes;
using System.Collections.Generic;

namespace Scouty.Models.Local
{
	[Table("Teams")]
	public class Team
	{
		[PrimaryKey, AutoIncrement, Column("_team")]
		public int Id { get; set; }

		[Column("team_number"), Unique]
		public int TeamNumber { get; set; }

		[MaxLength(100), Column("team_name")]
		public string TeamName { get; set; }

		[OneToMany(CascadeOperations = CascadeOperation.All), Column("robot_performances")]
		public List<RobotPerformance> Performances { get; set; }
	}
}

