using System;
using Scouty.Azure;

using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace Scouty.Models.Local
{
	[Table("RobotEvents")]
	public class RobotEvent
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }

		[ForeignKey(typeof(RobotPerformance)), Column("performance_id")]
		public int PerformanceId { get; set; }

		[Column("event_time")]
		public EventTime EventTime { get; set; }

		[Column("event_type")]
		public EventType EventType { get; set; }
	}

	public enum EventTime {
		Auto = 1,
		Teleop = 2,
		Final = 3
	}

	public enum EventType {
		MakeLow = 1,
		MakeHigh = 2,
		MissLow = 3,
		MissHigh = 4,
		CrossOne = 5,
		CrossTwo = 6,
		CrossThree = 7,
		CrossFour = 8,
		CrossFive = 9,
		AssistOne = 10,
		AssistTwo = 11,
		AssistThree = 12,
		AssistFour = 13,
		AssistFive = 14,
		ReachDefense = 15,
		Challenge = 16,
		Hang = 17,
		Foul = 18,
		TechnicalFoul = 19,
		BlockedShot = 20
	}

	public static class RobotEventExtensions {
		public static ClientRobotEvent ToRemote(this RobotEvent ev){
			return new ClientRobotEvent { 
				EventTime = ev.EventTime,
				EventType = ev.EventType
			};
		}
	}
}

