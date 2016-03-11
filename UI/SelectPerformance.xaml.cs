using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Scouty.Models.Local;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Scouty.Database;
using SQLiteNetExtensions.Extensions;
using Scouty.Utility;

namespace Scouty.UI
{
	public partial class SelectPerformance : ContentPage
	{
		Logger logger = new Logger (typeof(SelectPerformance));
		public ObservableCollection<Team> Teams;

		public event Action NavigatedTo;

		public int MatchNumber { get; }
		public MatchType MatchType { get; }
		public string EventCode { get; }

		public SelectPerformance (IEnumerable<Team> teams, int matchNumber, MatchType type, string eventCode)
		{
			InitializeComponent ();

			Teams = new ObservableCollection<Team> (teams);
			TeamsList.ItemsSource = Teams;

			TeamsList.ItemSelected += TeamSelected;
			MatchNumber = matchNumber;
			MatchType = type;
			EventCode = eventCode;
		}

		async void TeamSelected (object sender, SelectedItemChangedEventArgs e)
		{
			// Simply navigate to a team performance page
			var tcs = new TaskCompletionSource<RobotPerformance>();

			var page = new PerformancePage (e.SelectedItem as Team, MatchNumber, MatchType, EventCode);
			page.PerformanceCreated += tcs.SetResult;
			NavigatedTo += () => {
				if (!tcs.Task.IsCanceled || !tcs.Task.IsCompleted)
					tcs.TrySetCanceled ();
			};

			// Lets try to create the performance
			await Navigation.PushAsync(page);
			try {
				var perf = await tcs.Task;

				// Lets add it to the DB
				var db = LocalDatabase.Database.Connection;
				db.InsertAll(perf.Events);
				db.Insert(perf);
				db.UpdateWithChildren(perf);
				perf.Team.Performances.Add(perf);
				db.UpdateWithChildren(perf.Team);

				logger.Debug("Added performance to team!");

				// Go back twice
				await Navigation.PopAsync();
				await Navigation.PopAsync();
			} catch (OperationCanceledException){
				// We dont care about this
			}
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();

			NavigatedTo?.Invoke ();
		}
	}
}

