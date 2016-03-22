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

		public Team Team { get; }

		public int MatchNumber { get; }
		public MatchType MatchType { get; }
		public string EventCode { get; }
		public IList<DefenseType> Defenses { get; }

		public PerformancePage (Team team, int matchNumber, MatchType type, string eventCode, IList<DefenseType> defenses)
		{
			InitializeComponent ();
			Team = team;
			MatchNumber = matchNumber;
			MatchType = type;
			EventCode = eventCode;
			Title = $"Team {team.TeamNumber} : Match {matchNumber}";

			CurrentPage = this.Children[1];
			Defenses = defenses;

			// Set the texts
			CategoryACross.Text = defenses [1].GetDefenseTypeString ();
			CategoryBCross.Text = defenses [2].GetDefenseTypeString ();
			CategoryCCross.Text = defenses [3].GetDefenseTypeString ();
			CategoryDCross.Text = defenses [4].GetDefenseTypeString ();
			CategoryAAssist.Text = defenses [1].GetDefenseTypeString ();
			CategoryBAssist.Text = defenses [2].GetDefenseTypeString ();
			CategoryCAssist.Text = defenses [3].GetDefenseTypeString ();
			CategoryDAssist.Text = defenses [4].GetDefenseTypeString ();

			// Setup the buttons
			// Crosses
			LowBarCross.Clicked += (object sender, EventArgs e) => AddDefenseEvent(DefenseType.LowBar, true);
			CategoryACross.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[1], true);
			CategoryBCross.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[2], true);
			CategoryCCross.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[3], true);
			CategoryDCross.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[4], true);
			AssistedCross.Clicked += (object sender, EventArgs e) => AddEventAndIncrement(CrossCount, EventType.AssistedCross);

			// Assists
			LowBarAssist.Clicked += (object sender, EventArgs e) => AddDefenseEvent(DefenseType.LowBar, false);
			CategoryAAssist.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[1], false);
			CategoryBAssist.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[2], false);
			CategoryCAssist.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[3], false);
			CategoryDAssist.Clicked += (object sender, EventArgs e) => AddDefenseEvent(defenses[4], false);

			// Add Fouls
			Foul.Clicked += (object sender, EventArgs e) => AddEventAndIncrement(FoulCount, EventType.Foul);
			TechnicalFoul.Clicked += (object sender, EventArgs e) => AddEventAndIncrement(FoulCount, EventType.TechnicalFoul);

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
		}

		async void SubmitButton_Clicked (object sender, EventArgs e)
		{
			TaskCompletionSource<RobotPerformance> tcs = new TaskCompletionSource<RobotPerformance> ();
			ConfirmPerformancePage page = new ConfirmPerformancePage (events, Team, MatchNumber, MatchType, EventCode);

			NavigatedTo += () => {if (!tcs.Task.IsCanceled || !tcs.Task.IsCompleted) tcs.TrySetCanceled();};
			page.ConfirmedPerformance += tcs.SetResult;

			await Navigation.PushAsync (page);

			try {
				var performance = await tcs.Task;
				PerformanceCreated?.Invoke(performance);
			} catch (OperationCanceledException){
				// Dont care here
			}
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();

			NavigatedTo?.Invoke ();
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
		/// Adds an event and updates the label given
		/// </summary>
		/// <param name="label">Label.</param>
		/// <param name="type">Type.</param>
		void AddEventAndIncrement(Label label, EventType type){
			label.Text = int.Parse (label.Text) + 1 + "";
			AddEvent (type);
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
				AddEventAndIncrement (CrossCount, ev);
			else
				AddEventAndIncrement (AssistCount, ev);
		}

		/// <summary>
		/// Adds an Event
		/// </summary>
		/// <param name="type">Type.</param>
		void AddEvent(EventType type){
			events.Add (new RobotEvent{ EventTime = currentTime, EventType = type });
		}
	}
}

