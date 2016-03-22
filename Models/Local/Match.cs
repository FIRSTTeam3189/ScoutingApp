using System;

using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;
using System.Collections.Generic;
using Scouty.Azure;

namespace Scouty.Models.Local
{
	[Table("Matches")]
	public class Match
	{
		[PrimaryKey, AutoIncrement, Column("_id")]
		public int Id { get; set; }

		[Column("match_number")]
		public int MatchNumber { get; set; }

		[Column("match_type")]
		public MatchType MatchType { get; set; }

		[ForeignKey(typeof(Event)), Column("event_id")]
		public int EventId { get; set; }

		[Column("red_one")]
		public int RedOne { get; set; }

		[Column("red_two")]
		public int RedTwo { get; set; }

		[Column("red_three")]
		public int RedThree { get; set; }

		[Column("blue_one")]
		public int BlueOne { get; set; }

		[Column("blue_two")]
		public int BlueTwo { get; set; }

		[Column("blue_three")]
		public int BlueThree { get; set; }

		[Column("start_time")]
		public DateTime StartTime { get; set; }

		[Column("has_graded")]
		public bool HasGraded { get; set; }
	}

	public static class MatchExtensions {
		public static Match GetFromRemote(this ClientMatch match){
			System.DateTime dtDateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc);
			dtDateTime = dtDateTime.AddSeconds( match.Time ).ToLocalTime();
			return new Match{ 
				BlueOne = match.BlueOne,
				BlueThree = match.BlueThree,
				BlueTwo = match.BlueTwo,
				MatchNumber = match.MatchNumber,
				MatchType = match.MatchType,
				RedOne = match.RedOne,
				RedThree = match.RedThree,
				RedTwo = match.RedTwo,
				StartTime = dtDateTime
			};
		}
	}

	public enum MatchType {
		Practice = 1,
		Qualification = 2,
		OctoFinal = 3,
		QuarterFinal = 4,
		SemiFinal = 5,
		Final = 6
	}

	public enum DefenseType {
		Portcullis,
		ChevalDeFrise,
		Moat,
		Ramparts,
		Drawbridge,
		SallyPort,
		RockWall,
		RoughTerrain,
		LowBar
	}

	public enum DefenseCategory {
		A,
		B,
		C,
		D,
		LowBar
	}
}

