using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using Scouty.Database;
using Scouty.Models.Local;
using Xamarin.Forms;

namespace Scouty.UI
{
	public partial class MatchesPage : ContentPage
	{
		ObservableCollection<GroupedMatches> Matches { get; set; }
		public MatchesPage (string eventCode, int year)
		{
			InitializeComponent ();
			var matches = LocalDatabase.Database.QueryMatches (eventCode, year);

			// Lets group the matches
			Matches = new ObservableCollection<GroupedMatches> (matches
				.GroupBy (x => x.MatchType, (k, m) => new GroupedMatches (k, m)));
			MatchList.ItemsSource = Matches;
		}
	}

	public class MatchUI {
		public string Name { get; set; }
		public string Detail { get; set; }

		public Match Match { get; set; }

		public MatchUI(Match match){
			Name = $"Match {match.MatchNumber}";
			if (match.HasGraded)
				Detail = "Graded";
			else
				Detail = match.StartTime.ToString ("f");
		}
	}

	public class GroupedMatches : ObservableCollection<MatchUI>{
		public string ShortName { get; set; }
		public string LongName { get; set; }

		public MatchType Type { get; set; }

		public GroupedMatches(MatchType type, IEnumerable<Match> matches){
			foreach (var match in matches) {
				if (match.MatchType != type)
					throw new ArgumentException ("Match type Mismatch");
				Add (new MatchUI(match));
			}

			if (type == MatchType.Final) {
				ShortName = "F";
				LongName = "Finals";
			} else if (type == MatchType.OctoFinal) {
				ShortName = "OF";
				LongName = "Octo Finals";
			} else if (type == MatchType.Practice) {
				ShortName = "P";
				LongName = "Practice";
			} else if (type == MatchType.Qualification) {
				ShortName = "Q";
				LongName = "Qualification";
			} else if (type == MatchType.QuarterFinal) {
				ShortName = "QF";
				LongName = "Quarter Final";
			} else {
				ShortName = "SF";
				LongName = "Semi Finals";
			}
		}
	}
}

