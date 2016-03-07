using System;

using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace Scouty.Models.Local
{
	[Table("EventTeam")]
	public class EventTeam
	{
		[ForeignKey(typeof(Team)), Column("team_id")]
		public int TeamId { get; set; }

		[ForeignKey(typeof(Event)), Column("event_id")]
		public int EventId { get; set; }
	}
}

