using System;

using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;

namespace Scouty.Models.Local
{
	[Table("Events")]
	public class Event
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }

		[MaxLength(20), Column("event_code")]
		public string EventCode { get; set; }

		[Column("year")]
		public int Year { get; set; }

		[OneToMany(CascadeOperations = CascadeOperation.All), Column("matches")]
		public List<Match> Matches { get; set; }

		[ManyToMany(typeof(EventTeam), CascadeOperations = CascadeOperation.All), Column("teams")]
		public List<Team> Teams { get; set; }
	}
}

