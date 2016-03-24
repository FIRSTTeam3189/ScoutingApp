using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Forms;
using Scouty.Models.Local;
using Scouty.Utility;

namespace Scouty.UI
{
	public partial class PerformancePage : CarouselPage
	{
		List<RobotEvent> events = new List<RobotEvent> ();

		EventTime currentTime = EventTime.Auto;

		public event Action<RobotPerformance> PerformanceCreated;
		public event Action NavigatedTo;
		public event Action<IList<RobotEvent>> NavigatedBack;

		public Team Team { get; }

		public int MatchNumber { get; }
		public MatchType MatchType { get; }
		public string EventCode { get; }
		public IList<DefenseType> Defenses { get; }

		public PerformancePage (Team team, int matchNumber, MatchType type, string eventCode, IList<DefenseType> defenses, IList<Team> otherAlliance)
		{
			InitializeComponent ();
			Team = team;
			MatchNumber = matchNumber;
			MatchType = type;
			EventCode = eventCode;
			Page1Title.Text = $"Team {team.TeamNumber} : Match {matchNumber}";
			Page2Title.Text = $"Team {team.TeamNumber} : Match {matchNumber}";
			Page3Title.Text = $"Team {team.TeamNumber} : Match {matchNumber}";
			Page4Title.Text = $"Team {team.TeamNumber} : Match {matchNumber}";

			CurrentPage = this.Children[1];
			Defenses = defenses;
			events = new List<RobotEvent> ();

			// Save the state if needed
			Disappearing += (object sender, EventArgs e) => NavigatedBack?.Invoke(events);

			// Set the texts
			CategoryACross.Text = defenses [1].GetDefenseTypeString ();
			CategoryBCross.Text = defenses [2].GetDefenseTypeString ();
			CategoryCCross.Text = defenses [3].GetDefenseTypeString ();
			CategoryDCross.Text = defenses [4].GetDefenseTypeString ();
			CategoryAAssist.Text = defenses [1].GetDefenseTypeString ();
			CategoryBAssist.Text = defenses [2].GetDefenseTypeString ();
			CategoryCAssist.Text = defenses [3].GetDefenseTypeString ();
			CategoryDAssist.Text = defenses [4].GetDefenseTypeString ();
			BlockedOne.Text = otherAlliance [0].TeamNumber + "";
			BlockedTwo.Text = otherAlliance [1].TeamNumber + "";
			BlockedThree.Text = otherAlliance [2].TeamNumber + "";
			FailedBlockedOne.Text = otherAlliance [0].TeamNumber + "";
			FailedBlockedTwo.Text = otherAlliance [1].TeamNumber + "";
			FailedBlockedThree.Text = otherAlliance [2].TeamNumber + "";

			// Setup the buttons
			// Crosses
			LowBarCross.Clicked += (object sender, EventArgs e) => AddDefenseEvent(DefenseType.LowBar, true);
			CategoryACross.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[1], true);
			CategoryBCross.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[2], true);
			CategoryCCross.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[3], true);
			CategoryDCross.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[4], true);
			AssistedCross.Clicked += (object sender, EventArgs e) => AddEvent(EventType.AssistedCross);

			// Assists
			LowBarAssist.Clicked += (object sender, EventArgs e) => AddDefenseEvent(DefenseType.LowBar, false);
			CategoryAAssist.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[1], false);
			CategoryBAssist.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[2], false);
			CategoryCAssist.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[3], false);
			CategoryDAssist.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[4], false);

			// Add Fouls
			Foul.Clicked += (object sender, EventArgs e) => AddEvent(EventType.Foul);
			TechnicalFoul.Clicked += (object sender, EventArgs e) => AddEvent(EventType.TechnicalFoul);

			// Setup Block Buttions
			BlockedOne.Clicked += (object sender, EventArgs e) => AddEvent (EventType.BlockedShotOne);
			BlockedTwo.Clicked += (object sender, EventArgs e) => AddEvent (EventType.BlockedShotTwo);
			BlockedThree.Clicked += (object sender, EventArgs e) => AddEvent (EventType.BlockedShotThree);
			FailedBlockedOne.Clicked += (object sender, EventArgs e) => AddEvent (EventType.FailedBlockShotOne);
			FailedBlockedTwo.Clicked += (object sender, EventArgs e) => AddEvent (EventType.FailedBlockShotTwo);
			FailedBlockedThree.Clicked += (object sender, EventArgs e) => AddEvent (EventType.FailedBlockShotThree);

			// Setup Goal Buttons
			MakeHigh.Clicked += (object sender, EventArgs e) => AddEvent(EventType.MakeHigh);
			MakeLow.Clicked += (object sender, EventArgs e) => AddEvent (EventType.MakeLow);
			ContestedMakeHigh.Clicked += (object sender, EventArgs e) => AddEvent(EventType.MakeHighUnderPressure);
			ContestedMakeLow.Clicked += (object sender, EventArgs e) => AddEvent (EventType.MakeLowUnderPressure);
			MissHigh.Clicked += (object sender, EventArgs e) => AddEvent (EventType.MissHigh);
			MissLow.Clicked += (object sender, EventArgs e) => AddEvent (EventType.MissLow);
			ContestedMissHigh.Clicked += (object sender, EventArgs e) => AddEvent(EventType.MissHighUnderPressure);
			ContestedMissLow.Clicked += (object sender, EventArgs e) => AddEvent (EventType.MissLowUnderPressure);

			// Mode Button
			ModeButton.Clicked += (object sender, EventArgs e) => {
				if (currentTime == EventTime.Auto){
					currentTime = EventTime.Teleop;
					ModeButton.Text = "Teleop";
					ModeButton.BackgroundColor = Color.Lime;
				} else if (currentTime == EventTime.Teleop){
					currentTime = EventTime.Final;
					ModeButton.Text = "Final";
					ModeButton.BackgroundColor = Color.Fuchsia;
				} else {
					currentTime = EventTime.Auto;
					ModeButton.Text = "Autonomous";
					ModeButton.BackgroundColor = Color.Aqua;
				}
			};

			// Toggle Buttons
			ReachDefense.Clicked += (object sender, EventArgs e) => ToggleEvent(ReachDefense, EventType.ReachDefense);
			Challenge.Clicked += (object sender, EventArgs e) => ToggleEvent(Challenge, EventType.Challenge);
			YesHung.Clicked += (object sender, EventArgs e) => ToggleHang(false);
			NoHung.Clicked += (object sender, EventArgs e) => ToggleHang (true);

			// Other buttons
			Next.Clicked += SubmitButton_Clicked;
			Back.Clicked += async (object sender, EventArgs e) => await Navigation.PopModalAsync ();
		}

		/// <summary>
		/// Calculates the success fail label (0/0 labels).
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="success">Success.</param>
		/// <param name="fail">Fail.</param>
		void CalculateSuccessFailLabel(Label label, EventType success, EventType fail){
			var successes = events.Count (x => x.EventType == success);
			var fails = events.Count (x => x.EventType == fail);
			var total = successes + fails;

			label.Text = successes + "/" + total;
		}

		/// <summary>
		/// Calculates the counter label.
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="types">Types.</param>
		void CalculateCounterLabel(Label label, IEnumerable<EventType> types){
			var count = 0;
			foreach (var type in types) {
				count += events.Count (x => x.EventType == type);
			}

			label.Text = count + "";
		}

		/// <summary>
		/// Recalculates the labels.
		/// </summary>
		void RecalculateLabels(){
			// Calculate 0/0 Labels
			CalculateSuccessFailLabel (HighGoalCount, EventType.MakeHigh, EventType.MissHigh);
			CalculateSuccessFailLabel (ContestedHighGoalCount, EventType.MakeHighUnderPressure, EventType.MissHighUnderPressure);
			CalculateSuccessFailLabel (LowGoalCount, EventType.MakeLow, EventType.MissLow);
			CalculateSuccessFailLabel (ContestedLowGoalCount, EventType.MakeLowUnderPressure, EventType.MissLowUnderPressure);
			CalculateSuccessFailLabel (TeamOneBlockCount, EventType.BlockedShotOne, EventType.FailedBlockShotOne);
			CalculateSuccessFailLabel (TeamTwoBlockCount, EventType.BlockedShotTwo, EventType.FailedBlockShotTwo);
			CalculateSuccessFailLabel (TeamThreeBlockCount, EventType.BlockedShotThree, EventType.FailedBlockShotThree);

			// Calculate Counters
			CalculateCounterLabel (StealCount, new []{ EventType.StealBall });
			CalculateCounterLabel (FoulCount, new [] { EventType.Foul, EventType.TechnicalFoul });
			CalculateCounterLabel (CrossCount, new [] { EventType.CrossChevalDeFrise, EventType.CrossDrawBridge, EventType.CrossLowBar,
				EventType.CrossMoat, EventType.CrossPortcullis, EventType.CrossRamparts, EventType.CrossRockWall, EventType.CrossRoughTerrain,
				EventType.CrossSallyPort, EventType.AssistedCross
			});
			CalculateCounterLabel (AssistCount, new [] { EventType.AssistChevalDeFrise, EventType.AssistDrawBridge, EventType.AssistLowBar,
				EventType.AssistMoat, EventType.AssistPortcullis, EventType.AssistRamparts, EventType.AssistRockWall, EventType.AssistRoughTerrain,
				EventType.AssistSallyPort
			});

		}

		async void SubmitButton_Clicked (object sender, EventArgs e)
		{
			TaskCompletionSource<RobotPerformance> tcs = new TaskCompletionSource<RobotPerformance> ();
			ConfirmPerformancePage page = new ConfirmPerformancePage (events, Team, MatchNumber, MatchType, EventCode);

			NavigatedTo += () => {if (!tcs.Task.IsCanceled || !tcs.Task.IsCompleted) tcs.TrySetCanceled();};
			page.ConfirmedPerformance += tcs.SetResult;

			await Navigation.PushModalAsync (page);

			try {
				var performance = await tcs.Task;
				await Navigation.PopModalAsync();
				PerformanceCreated?.Invoke(performance);
			} catch (OperationCanceledException){
				// Dont care here
			} catch(Exception ex){
				Console.WriteLine (ex.Message);
			}
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();

			NavigatedTo?.Invoke ();
			RecalculateLabels ();
		}

		/// <summary>
		/// Toggles the hang.
		/// </summary>
		/// <param name="failedHang">If set to <c>true</c> failed hang.</param>
		void ToggleHang(bool failedHang){
			var existHang = events.FirstOrDefault (x => x.EventType == EventType.Hang || x.EventType == EventType.FailedHang);
			if (existHang != null)
				events.Remove (existHang);

			// Add hang
			if (failedHang) {
				AddEvent (EventType.FailedHang);
				YesHung.BackgroundColor = Color.Silver;
				NoHung.BackgroundColor = Color.Red;
			} else {
				AddEvent (EventType.Hang);
				YesHung.BackgroundColor = Color.Green;
				NoHung.BackgroundColor = Color.Silver;
			}
		}

		/// <summary>
		/// Toggles an Event in the list and sets button to appropriate color
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="type">Type.</param>
		void ToggleEvent(Button button, EventType type){
			var hasEvent = events.FirstOrDefault (x => x.EventType == type);
			if (hasEvent == null) {
				button.BackgroundColor = Color.Lime;
				AddEvent (type);
			} else {
				button.BackgroundColor = Color.Silver;
				events.Remove (hasEvent);
			}
		}

		/// <summary>
		/// Adds the defense event.
		/// </summary>
		/// <param name="type">Type.</param>
		/// <param name="isCross">If set to <c>true</c> is cross.</param>
		void AddDefenseEvent(DefenseType type, bool isCross){
			var ev = EventType.AssistChevalDeFrise;

			if (type == DefenseType.ChevalDeFrise) {
				ev = isCross ? EventType.CrossChevalDeFrise : EventType.AssistChevalDeFrise;
			} else if (type == DefenseType.Drawbridge) {
				ev = isCross ? EventType.CrossDrawBridge : EventType.AssistDrawBridge;
			} else if (type == DefenseType.LowBar) {
				ev = isCross ? EventType.CrossLowBar : EventType.AssistLowBar;
			} else if (type == DefenseType.Moat) {
				ev = isCross ? EventType.CrossMoat : EventType.AssistMoat;
			} else if (type == DefenseType.Portcullis) {
				ev = isCross ? EventType.CrossPortcullis : EventType.AssistPortcullis;
			} else if (type == DefenseType.Ramparts) {
				ev = isCross ? EventType.CrossRamparts : EventType.AssistRamparts;
			} else if (type == DefenseType.RockWall) {
				ev = isCross ? EventType.CrossRockWall : EventType.AssistRockWall;
			} else if (type == DefenseType.RoughTerrain) {
				ev = isCross ? EventType.CrossRoughTerrain : EventType.AssistRoughTerrain;
			} else if (type == DefenseType.SallyPort) {
				ev = isCross ? EventType.CrossSallyPort : EventType.AssistSallyPort;
			} 

			if (isCross)
				AddEvent (ev);
			else
				AddEvent (ev);
		}

		/// <summary>
		/// Adds an Event
		/// </summary>
		/// <param name="type">Type.</param>
		void AddEvent(EventType type){
			events.Add (new RobotEvent{ EventTime = currentTime, EventType = type });
			RecalculateLabels ();
		}

		async void SubmitClicked ()
		{
			
		}
	}
}

