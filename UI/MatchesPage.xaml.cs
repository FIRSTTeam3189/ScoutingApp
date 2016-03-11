using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using SQLiteNetExtensions.Extensions;

using Scouty.Database;
using Scouty.Models.Local;
using Scouty.Azure;
using Xamarin.Forms;
using Scouty.Utility;
using System.Threading.Tasks;

namespace Scouty.UI
{
	public partial class MatchesPage : ContentPage
	{
		Logger logger = new Logger (typeof(MatchesPage));
		ObservableCollection<GroupedMatches> Matches { get; set; }

		public string EventCode { get; }
		public int Year { get; }

		public event Action NavigatedTo;
		public MatchesPage (string eventCode, int year)
		{
			InitializeComponent ();
			var matches = LocalDatabase.Database.QueryMatches (eventCode, year);

			if (matches == null) {
				matches = new List<Match> ();
			}

			EventCode = eventCode;
			Year = year;

			// Lets group the matches
			Matches = new ObservableCollection<GroupedMatches> (matches
				.GroupBy (x => x.MatchType, (k, m) => new GroupedMatches (k, m)));
			MatchList.ItemsSource = Matches;
			MatchList.ItemSelected += Match_Selected;
			ToolbarItems.Add(new ToolbarItem("Other", null, OtherPressed));
		}

		async void Match_Selected (object sender, SelectedItemChangedEventArgs e)
		{
			// Get the match selected
			var match = (e.SelectedItem as MatchUI).Match;

			// Go to select performance page after getting all of the teams together
			var db = LocalDatabase.Database;
			var team1 = db.QueryTeam (match.RedOne);
			var team2 = db.QueryTeam (match.RedTwo);
			var team3 = db.QueryTeam (match.RedThree);
			var team4 = db.QueryTeam (match.BlueOne);
			var team5 = db.QueryTeam (match.BlueTwo);
			var team6 = db.QueryTeam (match.BlueThree);

			var page = new SelectPerformance (new List<Team> () { team1, team2, team3, team4, team5, team6 }, match.MatchNumber, match.MatchType, EventCode);
			await Navigation.PushAsync (page);
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			NavigatedTo?.Invoke ();
		}

		async void OtherPressed ()
		{
			var action = await DisplayActionSheet ("What would you like to do?", "Nothing", null, "Create Match", "Refresh Matches", "Syncronize");

			if (action == "Nothing")
				logger.Debug ("Nothing selected");
			else if (action == "Create Match") {
				// Create a match
				var tcs = new TaskCompletionSource<Match>();
				var page = new CreateMatchPage(EventCode, Year, Matches.Count == 0 ? 1 : Matches.Max(x => x.Max(y => y.Match.MatchNumber)) + 1);

				page.MatchCreated += tcs.SetResult;
				NavigatedTo += () => {
					if (!tcs.Task.IsCanceled || !tcs.Task.IsCompleted)
						tcs.TrySetCanceled ();
				};

				await Navigation.PushAsync (page);

				// Add it to collection and DB
				try {
					var match = await tcs.Task;
					var db = LocalDatabase.Database.Connection;
					var grp = Matches.FirstOrDefault(x => x.Type == match.MatchType);

					if (grp == null){
						grp = new GroupedMatches(match.MatchType, new List<Match>());
						Matches.Add(grp);
					}

					grp.Add(new MatchUI(match));

					// Get the event
					var ev = LocalDatabase.Database.QueryEvent(EventCode, Year, false);
					db.Insert(match);
					ev.Matches.Add(match);
					db.UpdateWithChildren(ev);

					// Go back to this page
					await Navigation.PopAsync();
				} catch (OperationCanceledException){
					// Ignore this shizniz
				}

			} else if (action == "Refresh Matches") {
				// Pull down matches from server
				// Login first
				logger.Info("Logging in...");
				var loginState = await UserManager.Login("jameswomack", "1234qwery");

				if (loginState != LoginState.Success){
					logger.Info("Failed to login: " + loginState);

					var registerState = await UserManager.CustomRegister("jameswomack", "1234qwery", "James Womack");

					if (registerState == RegisterState.Success){
						loginState = await UserManager.Login("jameswomack", "1234qwery");
						if (loginState == LoginState.Success)
							logger.Info ("Logged in!");
						else {
							logger.Error ("Failed to login: " + loginState);
							return;
						}
					} else {
						logger.Error("Failed to register: " + loginState);
						return;
					}
				}

				// Pull down the event
				var ev = await EventManager.RefreshEvents(Year);
				var thisEvent = ev.Where (x => x.EventCode == EventCode).Select(x => x.GetLocalEventFromRemote()).ToList();

				// Pull down the teams
				var teams = (await EventManager.TeamsForEvent(EventCode, Year)).Select(x => x.GetFromRemoteTeam(thisEvent)).ToList();

				// Pull down the matches
				var matches = await EventManager.MatchesForEvent(EventCode, Year);

				if (matches != null) {
					matches = new List<ClientMatch> ();
				}

				// Lets insert the data now!
				if (teams != null && thisEvent != null){
					Test.DeleteAllMatches (EventCode, Year);

					var trueMatches = matches.Select (x => x.GetFromRemote ()).ToList ();
					var db = LocalDatabase.Database.Connection;
					db.InsertOrReplaceAll (thisEvent);
					db.InsertAll (matches);

					foreach (var team in teams) {
						LocalDatabase.Database.AddTeam (team);
					}

					foreach (var e in thisEvent) {
						e.Teams.AddRange (teams);
						e.Matches.AddRange (trueMatches);
					}

					// Update the event now
					db.UpdateWithChildren(thisEvent.First());

					// Remove all the old ones
					Matches.Clear();
					var newMatches = new ObservableCollection<GroupedMatches> (trueMatches
						.GroupBy (x => x.MatchType, (k, m) => new GroupedMatches (k, m)));
					foreach (var grps in newMatches)
						Matches.Add (grps);

					await DisplayAlert ("Refreshed Events!", "Finished getting teams here!", "OK");
				}

			} else if (action == "Syncronize") {
				// Put performances on server
			} else
				logger.Error ("This shouldn't happen");
		}
	}

	public class MatchUI {
		public string Name { get; set; }
		public string Detail { get; set; }

		public Match Match { get; set; }

		public MatchUI(Match match){
			Name = $"Match {match.MatchNumber}";
			Match = match;
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

