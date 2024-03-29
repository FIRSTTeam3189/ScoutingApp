﻿using System;
using System.Linq;
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
		public IList<Team> RedAlliance { get; }
		public IList<Team> BlueAlliance { get; }

		public SelectPerformance (IEnumerable<Team> teams, int matchNumber, MatchType type, string eventCode, IList<Team> redAlliance, IList<Team> blueAlliance)
		{
			InitializeComponent ();

			Teams = new ObservableCollection<Team> (teams);
			TeamsList.ItemsSource = Teams;

			RedAlliance = redAlliance;
			BlueAlliance = blueAlliance;

			TeamsList.ItemSelected += TeamSelected;
			MatchNumber = matchNumber;
			MatchType = type;
			EventCode = eventCode;
		}

		async void TeamSelected (object sender, SelectedItemChangedEventArgs e)
		{
			var team = e.SelectedItem as Team;
			if (team == null)
				return;

			TeamsList.SelectedItem = null;

			// Simply navigate to a team performance page
			var tcs = new TaskCompletionSource<RobotPerformance>();

			// Get the other alliance
			IList<Team> otherAlliance;
			if (RedAlliance.FirstOrDefault (x => x.TeamNumber == team.TeamNumber) != null)
				otherAlliance = BlueAlliance;
			else
				otherAlliance = RedAlliance;

			var page = new SelectDefensePage (team, MatchNumber, MatchType, EventCode, otherAlliance);
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

				// Go back thrice
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

