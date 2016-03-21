using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Forms;
using Scouty.Models.Local;

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

		public PerformancePage (Team team, int matchNumber, MatchType type, string eventCode)
		{
			InitializeComponent ();
			Team = team;
			MatchNumber = matchNumber;
			MatchType = type;
			EventCode = eventCode;
			Title = $"Team {team.TeamNumber} : Match {matchNumber}";

			CurrentPage = this.Children[1];
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
		void AddCross(Label label, EventType type){
			label.Text = int.Parse (label.Text) + 1 + "";
			AddEvent (type);
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

