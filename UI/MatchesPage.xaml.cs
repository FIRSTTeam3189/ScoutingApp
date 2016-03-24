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
			ViewPerformancesButton.Clicked += ViewPerformances_Clicked;
			ToolbarItems.Add(new ToolbarItem("Other", null, OtherPressed));
		}

		async void ViewPerformances_Clicked (object sender, EventArgs e)
		{
			// Create the page
			var performancePage = new ViewPerformancesPage(EventCode);
			await Navigation.PushAsync (performancePage);
		}

		async void Match_Selected (object sender, SelectedItemChangedEventArgs e)
		{
			var matchUI = e.SelectedItem as MatchUI;

			if (matchUI == null)
				return;

			MatchList.SelectedItem = null;

			// Get the match selected
			var match = matchUI.Match;

			// Go to select performance page after getting all of the teams together
			var db = LocalDatabase.Database;
			var team1 = db.QueryTeam (match.RedOne);
			var team2 = db.QueryTeam (match.RedTwo);
			var team3 = db.QueryTeam (match.RedThree);
			var team4 = db.QueryTeam (match.BlueOne);
			var team5 = db.QueryTeam (match.BlueTwo);
			var team6 = db.QueryTeam (match.BlueThree);

			var redAlliance = new List<Team> () {
				team1, team2, team3
			};
			var blueAlliance = new List<Team> () { 
				team4, team5, team6
			};

			var page = new SelectPerformance (new List<Team> () { team1, team2, team3, team4, team5, team6 }, 
				           match.MatchNumber, match.MatchType, EventCode, redAlliance, blueAlliance);
			await Navigation.PushAsync (page);
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();
			NavigatedTo?.Invoke ();
		}

		async void OtherPressed ()
		{
			var action = await DisplayActionSheet ("What would you like to do?", "Nothing", null, "Create Match", "Refresh Matches", "Syncronize", "Test Get Performances");

			if (action == "Nothing")
				logger.Debug ("Nothing selected");
			else if (action == "Create Match") {
				// Create a match
				var tcs = new TaskCompletionSource<Match> ();
				var page = new CreateMatchPage (EventCode, Year, Matches.Count == 0 ? 1 : Matches.Max (x => x.Max (y => y.Match.MatchNumber)) + 1);

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
					var grp = Matches.FirstOrDefault (x => x.Type == match.MatchType);

					if (grp == null) {
						grp = new GroupedMatches (match.MatchType, new List<Match> ());
						Matches.Add (grp);
					}

					grp.Add (new MatchUI (match));

					// Get the event
					var ev = LocalDatabase.Database.QueryEvent (EventCode, Year, false);
					db.Insert (match);
					ev.Matches.Add (match);
					db.UpdateWithChildren (ev);

					// Go back to this page
					await Navigation.PopAsync ();
				} catch (OperationCanceledException) {
					// Ignore this shizniz
				}

			} else if (action == "Test Get Performances") {
				await DoScoutingStuffs ();
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

				if (matches == null) {
					matches = new List<ClientMatch> ();
				}

				// Lets insert the data now!
				if (teams != null && thisEvent != null){
					Test.DeleteAllMatches (EventCode, Year);

					var trueMatches = matches.OrderBy(x => x.MatchNumber).Select (x => x.GetFromRemote ()).ToList ();
					var db = LocalDatabase.Database.Connection;
					db.InsertOrReplaceAll (thisEvent);
					db.InsertAll (trueMatches);

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

				// Put performances on server
				var perfs = LocalDatabase.Database.GetPerformances(EventCode);
				var didIt = await PerformanceManager.PostPerformances (perfs.ToList());

				if (didIt)
					await DisplayAlert ("Syncronize", "Completed Syncronize", "OK");
				else
					await DisplayAlert ("Syncronize", "Failed", "OK");
			} else
				logger.Error ("This shouldn't happen");
		}

		async Task DoScoutingStuffs ()
		{
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

			var perfs = (await PerformanceManager.GetPerformances (EventCode)).Where(x => x.MatchType != MatchType.Practice).ToList();
			if (perfs != null) {
				var groupedPerfs = perfs.GroupBy (x => x.TeamId, (k, x) => new GroupedClientPerformance (k, x.ToList ())).ToList();

				var mostActive = groupedPerfs.OrderByDescending (x => x.AverageEventCount);
				var mostCrossing = groupedPerfs.OrderByDescending (x => x.AverageCrossCount);
				var mostShooting = groupedPerfs.OrderByDescending (x => x.AverageShootCount);
				logger.Info ("\n\n\n---------------------");
				logger.Info ("|     MOST ACTIVE   |");
				logger.Info ("---------------------\n");
				foreach (var grp in mostActive) {
					PrintPerformance (grp);
				}

				logger.Info ("\n\n\n---------------------");
				logger.Info ("|   MOST CROSSING  |");
				logger.Info ("---------------------\n");
				foreach (var grp in mostCrossing) {
					PrintPerformance (grp);
				}

				logger.Info ("\n\n\n---------------------");
				logger.Info ("|   MOST SHOOTING  |");
				logger.Info ("---------------------\n");
				foreach (var grp in mostShooting) {
					PrintPerformance (grp);
				}
			}
		}

		public void PrintPerformance(GroupedClientPerformance perf){
			logger.Info ($"Team Number: {perf.TeamNumber}");
			logger.Info ("\n---- Event Counts ------ ");
			logger.Info ($"Event Count: {perf.EventCount}");
			logger.Info ($"Performance Count: {perf.PerformanceCount}");
			logger.Info ($"Shoot Count: {perf.ShootCount}");
			logger.Info ($"High Shot Count: {perf.HighShotCount}");
			logger.Info ($"Low Shot Count: {perf.HighShotCount}");
			logger.Info ($"Challenge Count: {perf.ChallengeCount}");
			logger.Info ($"Autonomous Count: {perf.AutonomousCount}");
			logger.Info ($"Cross Count: {perf.CrossCount}\n");
			logger.Info ("----- Stat Info -----");
			logger.Info ($"Average Event Count: {perf.AverageEventCount}");
			logger.Info ($"Average Cross Count: {perf.AverageCrossCount}");
			logger.Info ($"Average Shot Count: {perf.AverageShootCount}");
			logger.Info ($"Average High Shot Count: {perf.AverageHighShotCount}");
			logger.Info ($"Average Low Shot Count: {perf.AverageLowShotCount}");
			logger.Info ($"Average Autonomous Count: {perf.AverageAutonomousCount}");
			logger.Info ($"Shooting Percentage High: {perf.ShootingPercentageHigh}");
			logger.Info ($"Shooting Percentage Low: {perf.ShootingPercentageLow}\n");
			logger.Info ($"Challenge Differential: {perf.ChallengeDifferential}");
			logger.Info ("***********************************************");
			foreach (var p in perf.Performances) {
				logger.Info ("------ Match: " + p.MatchNumber + ", Event Count: " + p.Events.Count);
			}
			logger.Info ("***********************************************\n\n\n");
		}
	}

	public struct GroupedClientPerformance {
		public GroupedClientPerformance(int teamNumber, List<ClientPerformance> perfs){
			// Get all of the robot Events
			var events = perfs.SelectMany(x => x.Events).ToList();
			TeamNumber = teamNumber;
			Performances = perfs;
			EventCount = perfs.Sum (y => y.Events.Count);
			PerformanceCount = perfs.Count == 0 ? 0 : perfs.Count;
			ShootCount = events.Count (x => x.EventType == EventType.MakeHigh || x.EventType == EventType.MissLow || x.EventType == EventType.MakeLow || x.EventType == EventType.MissHigh);
			HighShotCount = events.Count (x => x.EventType == EventType.MakeHigh || x.EventType == EventType.MissHigh);
			LowShotCount = events.Count (x => x.EventType == EventType.MakeLow || x.EventType == EventType.MissLow);
			ChallengeCount = events.Count (x => x.EventType == EventType.Challenge);
			AutonomousCount = events.Count (x => x.EventTime == EventTime.Auto || x.EventType == EventType.ReachDefense);
			CrossCount = 0;
			FoulCount = events.Count (x => x.EventType == EventType.Foul || x.EventType == EventType.TechnicalFoul);
			HangCount = events.Count (x => x.EventType == EventType.Hang);

			AverageEventCount = perfs.Average (y => y.Events.Count);
			AverageCrossCount = CrossCount / (double)PerformanceCount;
			AverageShootCount = ShootCount / (double)PerformanceCount;
			AverageHighShotCount = HighShotCount / (double)PerformanceCount;
			AverageLowShotCount = LowShotCount / (double)PerformanceCount;
			AverageAutonomousCount = AutonomousCount / (double)PerformanceCount;

			ShootingPercentageHigh = events.Count (x => x.EventType == EventType.MakeHigh) / (double)HighShotCount;
			ShootingPercentageLow = events.Count (x => x.EventType == EventType.MakeLow) / (double)LowShotCount;
			ChallengeDifferential = PerformanceCount - ChallengeCount;
		}

		public int TeamNumber { get; set; }
		public List<ClientPerformance> Performances { get; set; }
		public int EventCount { get; set; }
		public int CrossCount { get; set; }
		public int ShootCount { get; set; }
		public int HighShotCount { get; set; }
		public int LowShotCount { get; set; }
		public int AutonomousCount { get; set; }
		public int FoulCount { get; set; }
		public int ChallengeCount { get; set; }
		public int HangCount { get; set; }
		public int PerformanceCount { get; set; }

		public double AverageEventCount { get; set; }
		public double AverageCrossCount { get; set; }
		public double AverageShootCount { get; set; }
		public double AverageHighShotCount { get; set; }
		public double AverageLowShotCount { get; set; }
		public double AverageAutonomousCount { get; set; }
		public double ShootingPercentageHigh { get; set; }
		public double ShootingPercentageLow { get; set; }
		public int ChallengeDifferential { get; set; }
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

			ShortName = type.GetMatchTypeShortString ();
			LongName = type.GetMatchTypeString ();
		}
	}
}

