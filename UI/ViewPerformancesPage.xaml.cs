using System;
using System.Linq;
using System.Collections.Generic;

using Xamarin.Forms;
using System.Collections.ObjectModel;
using Scouty.Models.Local;
using Scouty.Database;

namespace Scouty.UI
{
	public partial class ViewPerformancesPage : ContentPage
	{
		public ObservableCollection<GroupedPerformance> Performances { get; set; }
		public ViewPerformancesPage (string eventCode)
		{
			InitializeComponent ();

			var perfs = LocalDatabase.Database.GetPerformances (eventCode).GroupBy (x => x.Team.TeamNumber, (k, x) => new GroupedPerformance(x, k)).OrderBy(x => x.TeamNumber);
			Performances = new ObservableCollection<GroupedPerformance> (perfs);

			PerformancesList.ItemsSource = Performances;
		}
	}

	public class GroupedPerformance : ObservableCollection<PerformanceUI> {
		public string ShortName { get; set; }
		public string LongName { get; set; }
		public int TeamNumber { get; set; }

		public GroupedPerformance(IEnumerable<RobotPerformance> performances, int teamNumber){
			TeamNumber = teamNumber;
			var teamName = LocalDatabase.Database.QueryTeam (teamNumber, false).TeamName;			
			foreach (var perf in performances) {
				if (perf.Team.TeamNumber != teamNumber)
					throw new ArgumentException ("Team number must match performance");

				Add (new PerformanceUI(perf));
			}

			ShortName = teamNumber + "";
			LongName = teamNumber + " - " + teamName;
		}
	}

	public class PerformanceUI {
		public RobotPerformance Performance { get; set; }
		public string Name { get; set; }
		public string Detail { get; set; }
		public PerformanceUI(RobotPerformance performance){
			Performance = performance;

			Name = $"Match {performance.MatchNumber}";
			if (performance.MatchType == MatchType.Final)
				Detail = "Final";
			else if (performance.MatchType == MatchType.OctoFinal)
				Detail = "Octo-Final";
			else if (performance.MatchType == MatchType.Practice)
				Detail = "Practice";
			else if (performance.MatchType == MatchType.Qualification)
				Detail = "Qualification";
			else if (performance.MatchType == MatchType.QuarterFinal)
				Detail = "Quarter Final";
			else if (performance.MatchType == MatchType.SemiFinal)
				Detail = "Semi-Final";
		}
	}
}

