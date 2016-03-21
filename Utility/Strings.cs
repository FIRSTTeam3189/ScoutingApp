using System;
using Scouty.Models.Local;

namespace Scouty.Utility
{
	public static class Strings
	{
		/// <summary>
		/// Gets the event type string.
		/// </summary>
		/// <returns>The event type string.</returns>
		/// <param name="ev">Ev.</param>
		/// <param name="includeTime">If set to <c>true</c> include time.</param>
		public static string GetEventTypeString(this RobotEvent ev, bool includeTime = false){
			string str = "";

			if (ev.EventType == EventType.AssistChevalDeFrise) {
				str = "Assisted a Robot across Chival De Frise";
			} else if (ev.EventType == EventType.AssistDrawBridge) {
				str = "Assisted a Robot across Draw Bridge";
			} else if (ev.EventType == EventType.AssistedCross) {
				str = "Assisted Cross a defense";
			} else if (ev.EventType == EventType.AssistLowBar) {
				str = "Assisted a Robot across Low bar";
			} else if (ev.EventType == EventType.AssistMoat) {
				str = "Assisted a Robot across Moat";
			} else if (ev.EventType == EventType.AssistPortcullis) {
				str = "Assisted a Robot across Portcullis";
			} else if (ev.EventType == EventType.AssistRamparts) {
				str = "Assisted a Robot across Ramparts";
			} else if (ev.EventType == EventType.AssistRockWall) {
				str = "Assisted a Robot across Rock Wall";
			} else if (ev.EventType == EventType.AssistRoughTerrain) {
				str = "Assisted a Robot across Rough Terrain";
			} else if (ev.EventType == EventType.AssistSallyPort) {
				str = "Assisted a Robot across Sally Port";
			} else if (ev.EventType == EventType.BlockedShotOne || ev.EventType == EventType.BlockedShotThree || ev.EventType == EventType.BlockedShotTwo) {
				str = "Blocked a Robot Shot";
			} else if (ev.EventType == EventType.FailedBlockShotOne || ev.EventType == EventType.FailedBlockShotThree || ev.EventType == EventType.FailedBlockShotTwo) {
				str = "Failed to block a Robot Shot";
			} else if (ev.EventType == EventType.Challenge) {
				str = "Challeneged the batter";
			} else if (ev.EventType == EventType.CrossChevalDeFrise) {
				str = "Crossed the Chival De Frise";
			} else if (ev.EventType == EventType.CrossDrawBridge) {
				str = "Crossed the Draw Bridge";
			} else if (ev.EventType == EventType.CrossLowBar) {
				str = "Crossed the Low Bar";
			} else if (ev.EventType == EventType.CrossMoat) {
				str = "Crossed the Moal";
			} else if (ev.EventType == EventType.CrossPortcullis) {
				str = "Crossed the Portcullis";
			} else if (ev.EventType == EventType.CrossRamparts) {
				str = "Crossed the Ramparts";
			} else if (ev.EventType == EventType.CrossRockWall) {
				str = "Crossed the Rock Wall";
			} else if (ev.EventType == EventType.CrossRoughTerrain) {
				str = "Crossed the Rough Terrain";
			} else if (ev.EventType == EventType.CrossSallyPort) {
				str = "Crossed the Sally Port";
			} else if (ev.EventType == EventType.CrossChevalDeFrise) {
				str = "Crossed the Chival De Frise";
			} else if (ev.EventType == EventType.Foul) {
				str = "Foul";
			} else if (ev.EventType == EventType.Hang) {
				str = "Robot is Hung";
			} else if (ev.EventType == EventType.MakeHigh) {
				str = "Made a High Shot";
			} else if (ev.EventType == EventType.MakeLow) {
				str = "Made a Low Shot";
			} else if (ev.EventType == EventType.MakeHighUnderPressure) {
				str = "Made a High Shot Under Pressure";
			} else if (ev.EventType == EventType.MakeLowUnderPressure) {
				str = "Made a Low Shot Under Pressure";
			} else if (ev.EventType == EventType.MissHigh) {
				str = "Missed a High Shot";
			} else if (ev.EventType == EventType.MissLow) {
				str = "Missed a Low Shot";
			} else if (ev.EventType == EventType.MissHighUnderPressure) {
				str = "Missed a High Shot Under Pressure";
			} else if (ev.EventType == EventType.MissLowUnderPressure) {
				str = "Missed a Low Shot Under Pressure";
			} else if (ev.EventType == EventType.ReachDefense) {
				str = "Robot Reached a Defense";
			} else if (ev.EventType == EventType.RobotFailure) {
				str = "Robot Broke Down";
			} else if (ev.EventType == EventType.StealBall) {
				str = "Stole a ball";
			} else if (ev.EventType == EventType.TechnicalFoul) {
				str = "Technical Foul";
			} else {
				str = "This is a bug...";
			}

			if (includeTime) {
				if (ev.EventTime == EventTime.Auto) {
					str += " during Autonomous";
				} else if (ev.EventTime == EventTime.Final) {
					str += " during Final";
				} else {
					str += " during Teleop";
				}
			}

			return str;
		}

		/// <summary>
		/// Gets the event time string.
		/// </summary>
		/// <returns>The event time string.</returns>
		/// <param name="time">Time.</param>
		public static string GetEventTimeString(this EventTime time){
			if (time == EventTime.Auto)
				return "Autonomous Period";
			else if (time == EventTime.Final)
				return "Final Period";
			else
				return "Teleop Period";
		}

		/// <summary>
		/// Gets the event time short string.
		/// </summary>
		/// <returns>The event time short string.</returns>
		/// <param name="time">Time.</param>
		public static string GetEventTimeShortString(this EventTime time){
			if (time == EventTime.Auto)
				return "A";
			else if (time == EventTime.Final)
				return "F";
			else
				return "T";
		}

		/// <summary>
		/// Gets the match type string.
		/// </summary>
		/// <returns>The match type string.</returns>
		/// <param name="type">Type.</param>
		public static string GetMatchTypeString(this MatchType type){
			if (type == MatchType.Final) {
				return "Finals";
			} else if (type == MatchType.OctoFinal) {
				return "Octo-Finals";
			} else if (type == MatchType.Practice) {
				return "Practice";
			} else if (type == MatchType.Qualification) {
				return "Qualifications";
			} else if (type == MatchType.QuarterFinal) {
				return "Quarter Finals";
			} else {
				return "Semi-Finals";
			}
		}

		/// <summary>
		/// Gets the match type short string.
		/// </summary>
		/// <returns>The match type short string.</returns>
		/// <param name="type">Type.</param>
		public static string GetMatchTypeShortString(this MatchType type){
			if (type == MatchType.Final) {
				return "F";
			} else if (type == MatchType.OctoFinal) {
				return "OF";
			} else if (type == MatchType.Practice) {
				return "P";
			} else if (type == MatchType.Qualification) {
				return "QM";
			} else if (type == MatchType.QuarterFinal) {
				return "QF";
			} else {
				return "SF";
			}
		}
	}
}

